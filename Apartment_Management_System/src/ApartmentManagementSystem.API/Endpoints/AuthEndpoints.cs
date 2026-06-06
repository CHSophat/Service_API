using ApartmentManagementSystem.Application.Command.AuthCommand;
using ApartmentManagementSystem.Application.DTOs.AuthDto;
using ApartmentManagementSystem.Application.Query.AuthQuery;
using ApartmentManagementSystem.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/auth/* (and /api/auth/* alias for backwards compatibility) –
/// authentication, sessions, 2FA, OTP, SSO, invitations.
/// Replaces the deleted AuthController. Used by both App (Mobile) and Web (MSI).
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Primary path; also bind /api/auth/* so existing clients don't break overnight.
        Register(app.MapGroup($"{EndpointRegistration.ApiBase}/auth").WithTags("Auth (Shared)"));
        Register(app.MapGroup("/api/auth").WithTags("Auth (legacy)"));
        return app;
    }

    private static void Register(RouteGroupBuilder g)
    {
        // ---- Public ----------------------------------------------------------
        g.MapPost("/register", async (RegisterRequest req, IMediator mediator) =>
        {
            try
            {
                var exists = await mediator.Send(new CheckUserExistsByEmailQuery { Email = req.Email });
                if (exists)
                    return ApiResponseExtensions.Conflict("Email already registered");

                var result = await mediator.Send(new RegisterUserCommand
                {
                    Email = req.Email,
                    Password = req.Password,
                    Phone = req.Phone ?? string.Empty
                });
                return ApiResponseExtensions.Created(result, "User registered successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapPost("/login", async (LoginRequest req, IAuthService auth) =>
        {
            try
            {
                var result = await auth.LoginAsync(req.Email, req.Password);
                if (!result.Succeeded)
                {
                    return result.Reason switch
                    {
                        AuthFailureReason.AccountLocked   => ApiResponseExtensions.Locked("Account locked due to multiple failed attempts"),
                        AuthFailureReason.AccountDisabled => ApiResponseExtensions.Unauthorized("Account is disabled"),
                        _                                  => ApiResponseExtensions.Unauthorized("Invalid email or password")
                    };
                }

                return ApiResponseExtensions.Ok(new AuthResponse
                {
                    UserId = result.UserId,
                    Email = result.Email,
                    Phone = result.Phone,
                    Roles = result.Roles.ToList(),
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    AccessTokenExpires = result.AccessTokenExpiresUtc,
                    Is2FaRequired = result.Is2FaRequired,
                    IsEmailVerified = result.IsEmailVerified
                }, "Login successful");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapPost("/refresh", async (RefreshTokenRequest req, IAuthService auth) =>
        {
            try
            {
                var result = await auth.RefreshAsync(req.RefreshToken ?? string.Empty);
                if (!result.Succeeded)
                    return ApiResponseExtensions.Unauthorized("Invalid or expired refresh token");

                return ApiResponseExtensions.Ok(new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.AccessTokenExpiresUtc
                }, "Token refreshed");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapPost("/logout", async (LogoutRequest req, HttpContext ctx, IAuthService auth) =>
        {
            try
            {
                var userId = GetUserId(ctx);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                await auth.LogoutAsync(userId, req.RefreshToken, req.LogoutAllDevices);
                return ApiResponseExtensions.Ok(new { }, req.LogoutAllDevices
                    ? "Logged out from all devices"
                    : "Logged out successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).RequireAuthorization();

        g.MapGet("/me", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(ctx);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var u = await mediator.Send(new GetCurrentUserQuery { UserId = userId });
                if (u is null) return ApiResponseExtensions.NotFound("User not found");

                // Derive the flattened permission codes from the role list so
                // the web client can hide/disable CRUD buttons. Backend still
                // enforces authz via RequireRole / RequireAuthorization.
                u.Permissions = Permissions.For(u.Roles).ToArray();
                return ApiResponseExtensions.Ok(u, "User profile retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).RequireAuthorization();

        // ---- Sessions --------------------------------------------------------
        g.MapGet("/sessions", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var userId = GetUserId(ctx);
                if (userId <= 0) return ApiResponseExtensions.Unauthorized();

                var sessions = await mediator.Send(new GetActiveSessionsQuery
                {
                    UserId = userId,
                    CurrentTokenHash = GetBearer(ctx)
                });
                return ApiResponseExtensions.Ok(sessions, "Sessions retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).RequireAuthorization();

        g.MapDelete("/sessions/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                var revoked = await mediator.Send(new RevokeRefreshTokenCommand { TokenId = id });
                return revoked
                    ? ApiResponseExtensions.Ok(new { }, "Session revoked")
                    : ApiResponseExtensions.NotFound("Session not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).RequireAuthorization();

        // ---- Verification, password, OTP, 2FA, SSO --------------------------
        g.MapPost("/verify-email", (VerifyEmailRequest _) =>
            EndpointResults.NotImplemented("verify email code, mark IsEmailVerified")).AllowAnonymous();

        g.MapPost("/password/forgot", (ForgotPasswordRequest _) =>
            EndpointResults.NotImplemented("generate reset token, send email")).AllowAnonymous();

        g.MapPost("/password/reset", (ResetPasswordRequest _) =>
            EndpointResults.NotImplemented("validate reset token, update password, revoke sessions")).AllowAnonymous();

        g.MapPost("/otp/request", async (OtpRequestDto req, IMediator mediator) =>
        {
            try
            {
                // TODO: rate-limit + resolve user by phone before creating OTP.
                var code = Random.Shared.Next(100000, 999999).ToString();
                var result = await mediator.Send(new CreateOtpCommand
                {
                    UserId = 0,
                    Phone = req.Phone,
                    Code = code,
                    Purpose = req.Purpose
                });
                return ApiResponseExtensions.Ok(result, "OTP sent");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapPost("/otp/verify", (OtpVerifyRequest _) =>
            EndpointResults.NotImplemented("verify OTP, mark used, issue tokens")).AllowAnonymous();

        g.MapPost("/2fa/enable", (Enable2FaRequest _) =>
            EndpointResults.NotImplemented("verify password, generate TOTP secret + backup codes")).RequireAuthorization();

        g.MapPost("/2fa/verify", (Verify2FaRequest _) =>
            EndpointResults.NotImplemented("verify TOTP, activate 2FA")).RequireAuthorization();

        g.MapPost("/2fa/login", (Submit2FaCodeRequest _) =>
            EndpointResults.NotImplemented("verify TOTP, issue tokens")).AllowAnonymous();

        g.MapPost("/2fa/backup-codes", () =>
            EndpointResults.NotImplemented("regenerate 2FA backup codes")).RequireAuthorization();

        g.MapPost("/sso/google", (SsoLoginRequest _) =>
            EndpointResults.NotImplemented("verify Google ID token, get-or-create user, mint tokens")).AllowAnonymous();

        // ---- Web-only: audit log, invitations -------------------------------
        g.MapGet("/audit-log", ([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? user) =>
            EndpointResults.NotImplemented("auth audit-log query (Web)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/invitations", () =>
            EndpointResults.NotImplemented("list pending invitations (Web)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/invitations", () =>
            EndpointResults.NotImplemented("create invitation, email tokenized link (Web)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/invitations/{id:int}/resend", (int id) =>
            EndpointResults.NotImplemented("resend invitation email (Web)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapDelete("/invitations/{id:int}", (int id) =>
            EndpointResults.NotImplemented("revoke invitation (Web)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        // ---- DIAGNOSTIC ------------------------------------------------------
        // Anonymous so it returns even when JwtBearer rejects the token. Probes
        // what the server sees when validating the Authorization header.
        // Use this to debug "I pasted a token but still get 401".
        g.MapGet("/_debug-token", async (HttpContext ctx) =>
        {
            var rawHeader = ctx.Request.Headers["Authorization"].ToString();
            var token = rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? rawHeader["Bearer ".Length..]
                : rawHeader;

            var result = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

            return Results.Json(new
            {
                receivedAuthorizationHeader = rawHeader.Length == 0 ? "(none)" : rawHeader[..Math.Min(20, rawHeader.Length)] + "...",
                receivedTokenLength = token.Length,
                authenticated = result.Succeeded,
                failureMessage = result.Failure?.Message,
                failureType = result.Failure?.GetType().Name,
                identityName = result.Principal?.Identity?.Name,
                isAuthenticated = result.Principal?.Identity?.IsAuthenticated ?? false,
                authenticationType = result.Principal?.Identity?.AuthenticationType,
                claims = result.Principal?.Claims
                    .Select(c => new { type = c.Type, value = c.Value })
                    .ToList(),
                hint = result.Succeeded
                    ? "Token is valid. If protected endpoints still 401, check the role gates (see /docs/API_APP_VS_WEB.md)."
                    : "Token did not validate. Common causes: API was restarted after this token was issued (signing key now differs), token expired, Issuer/Audience mismatch in appsettings, or no \"Bearer \" prefix in the Authorization header."
            });
        }).AllowAnonymous();
    }

    private static int GetUserId(HttpContext ctx)
    {
        // Tolerant of older tokens (URI claim type) and current ones (short "sub").
        var claim = ctx.User.FindFirst("sub")
                 ?? ctx.User.FindFirst("userId")
                 ?? ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                 ?? ctx.User.FindFirst("nameid");
        return int.TryParse(claim?.Value, out var id) ? id : 0;
    }

    private static string GetBearer(HttpContext ctx)
    {
        var h = ctx.Request.Headers["Authorization"].ToString();
        return h.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? h["Bearer ".Length..]
            : string.Empty;
    }
}
