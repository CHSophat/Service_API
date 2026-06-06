using ApartmentManagementSystem.Application.Command.SettingsCommand;
using ApartmentManagementSystem.Application.DTOs.SettingsDto;
using ApartmentManagementSystem.Application.Query.SettingsQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/settings/* + /api/v1/audit-log – Web-only PM settings, branding, audit.
/// </summary>
public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var s = app.MapGroup($"{EndpointRegistration.ApiBase}/settings")
                   .WithTags("Settings (Web)")
                   .RequireAuthorization(p => p.RequireRole(
                       AppRoles.PropertyManager, AppRoles.Admin));

        s.MapGet("/branding", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBrandingQuery());
            return ApiResponseExtensions.Ok(result, "Branding settings retrieved");
        });

        s.MapPut("/branding", async ([FromBody] UpdateBrandingRequest req, IMediator mediator) =>
        {
            try
            {
                var cmd = new UpdateBrandingCommand
                {
                    OrgName = req.OrgName,
                    PrimaryColor = req.PrimaryColor,
                    AccentColor = req.AccentColor,
                    ContactEmail = req.ContactEmail,
                    ContactPhone = req.ContactPhone,
                    Locale = req.Locale,
                };
                var result = await mediator.Send(cmd);
                return ApiResponseExtensions.Ok(result, "Branding updated");
            }
            catch (Exception ex)
            {
                return ApiResponseExtensions.Error(ex);
            }
        });

        s.MapPost("/branding/logo", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var result = await mediator.Send(new UploadBrandingLogoCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                });
                return ApiResponseExtensions.Ok(result, "Logo uploaded");
            }
            catch (ArgumentException ex)
            {
                return ApiResponseExtensions.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponseExtensions.Error(ex);
            }
        })
        .DisableAntiforgery();

        app.MapGet($"{EndpointRegistration.ApiBase}/audit-log", async (
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] int? userId, [FromQuery] string? action, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAuditLogQuery
            {
                From = from,
                To = to,
                UserId = userId,
                Action = action,
            });
            return ApiResponseExtensions.Ok(result, "Audit log retrieved");
        })
        .WithTags("Audit Log (Web)")
        .RequireAuthorization(p => p.RequireRole(
            AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        return app;
    }
}
