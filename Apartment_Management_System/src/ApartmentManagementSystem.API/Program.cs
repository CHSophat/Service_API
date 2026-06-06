using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ApartmentManagementSystem.API.Authorization;
using ApartmentManagementSystem.API.Endpoints;
using ApartmentManagementSystem.API.Swagger;
using ApartmentManagementSystem.Application;
using ApartmentManagementSystem.Infrastructure;
using ApartmentManagementSystem.Infrastructure.Security;
using ApartmentManagementSystem.Infrastructure.Services;
using SmtpSettings = ApartmentManagementSystem.Infrastructure.Services.SmtpSettings;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddApplicationServices();

// Configure Token Settings from appsettings (accept either key spelling)
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Missing JWT secret. Set Jwt:Secret in appsettings.");
var tokenSettings = new TokenSettings
{
    SecretKey = jwtSecret,
    ExpirationMinutes = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? builder.Configuration["Jwt:ExpirationMinutes"] ?? "60"),
    RefreshTokenExpirationDays = int.Parse(builder.Configuration["Jwt:RefreshTokenExpiryDays"] ?? builder.Configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"),
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "ApartmentManagementSystem",
    Audience = builder.Configuration["Jwt:Audience"] ?? "ApartmentManagementSystemAPI"
};

// Surface the *effective* JWT config on startup so that, when a token is
// rejected, you can confirm both endpoints (issuer + audience + key) actually
// agree. A SHA-256 prefix of the secret lets us compare keys safely without
// printing them.
var keyFingerprint = Convert.ToHexString(
    System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)))[..16];
Console.WriteLine("========== JWT effective config ==========");
Console.WriteLine($" Issuer       : {tokenSettings.Issuer}");
Console.WriteLine($" Audience     : {tokenSettings.Audience}");
Console.WriteLine($" Secret length: {tokenSettings.SecretKey.Length} chars");
Console.WriteLine($" Secret SHA256: {keyFingerprint}…  (compare across restarts)");
Console.WriteLine($" Access TTL   : {tokenSettings.ExpirationMinutes} min");
Console.WriteLine($" Refresh TTL  : {tokenSettings.RefreshTokenExpirationDays} day(s)");
Console.WriteLine("==========================================");

// Configure SMTP Settings from appsettings
var smtpSettings = new SmtpSettings
{
    Host = builder.Configuration["Smtp:Host"],
    Port = int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
    Username = builder.Configuration["Smtp:Username"],
    Password = builder.Configuration["Smtp:Password"],
    FromEmail = builder.Configuration["Smtp:FromEmail"],
    FromName = builder.Configuration["Smtp:FromName"],
    EnableSsl = bool.Parse(builder.Configuration["Smtp:EnableSsl"] ?? "true")
};

// Register Infrastructure Services
builder.Services.AddInfrastructureServices(
    builder.Configuration.GetConnectionString("ApartmentDbSystem"),
    tokenSettings,
    smtpSettings);

// JWT Authentication.
//
// IMPORTANT: clear the legacy short→long inbound claim map so JWT short names
// ("sub", "email", "role") stay as-is instead of being rewritten to URIs like
// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier".
// Without this, the token has "sub" but ClaimsPrincipal exposes the URI form,
// which breaks any code that looks up claims by their JWT short name.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        // Keep the short JWT names — do NOT remap them.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = tokenSettings.Issuer,
            ValidAudience            = tokenSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
            // Authoritative claim names for [Authorize(Roles=...)] / User.Identity.Name.
            NameClaimType            = "sub",
            RoleClaimType            = "role",
            ClockSkew                = TimeSpan.FromMinutes(2)
        };

        // Surface the *real* validation failure to the caller (Swagger / Postman)
        // via the WWW-Authenticate header + a JSON body — instead of an empty 401.
        // Helps debugging when "I pasted my token and still get Unauthorized".
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.Headers["x-token-error"] = ctx.Exception.GetType().Name;
                return Task.CompletedTask;
            },
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";
                var reason = ctx.AuthenticateFailure?.Message
                             ?? (string.IsNullOrEmpty(ctx.Request.Headers.Authorization)
                                 ? "Missing Authorization header. Click the Authorize button and paste a JWT."
                                 : "Token is missing, expired, or invalid.");
                var payload = JsonSerializer.Serialize(new
                {
                    status     = "error",
                    statusCode = 401,
                    message    = reason,
                    hint       = "POST /api/v1/auth/login to obtain a fresh access token, then paste it into Swagger's Authorize dialog (no \"Bearer \" prefix)."
                });
                await ctx.Response.WriteAsync(payload);
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    // Every endpoint requires an authenticated user by default.
    // Endpoints explicitly marked [AllowAnonymous] (register, login, refresh, etc.) bypass this.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();

// Gzip/Brotli compression for JSON responses — cuts payload size over the wire.
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Apartment Management System API",
        Version = "v1",
        Description = "AMS backend. Mobile (App) routes use Tenant/Owner roles; Web (MSI) routes use PropertyManager/Admin/Staff roles."
    });

    // Defines the JWT Bearer scheme that backs the top-right "Authorize" button.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste only the JWT (no \"Bearer \" prefix). Get one from POST /api/v1/auth/login. " +
                       "Click \"Logout\" in the same dialog to clear the token (Unauthorize)."
    });

    // Per-operation lock state. AuthorizeOperationFilter inspects each
    // endpoint's metadata: AllowAnonymous → open padlock, otherwise the
    // Bearer requirement (closed padlock) is attached and required roles are
    // surfaced in the description.
    c.OperationFilter<AuthorizeOperationFilter>();
});

// CORS for React
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReact",
		policy =>
		{
			policy.WithOrigins(
					  "http://localhost:3000",
					  "http://localhost:5173",
					  "http://localhost:4200")
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});

var app = builder.Build();

// --------------------
// Middleware
// --------------------

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Display URLs and open browser to swagger UI when the app starts (development only)
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            // Get the first served URL
            var url = app.Urls.FirstOrDefault() ?? "https://localhost:5001";
            var swaggerUrl = url.TrimEnd('/') + "/swagger";
            
            // Display URLs in console
            Console.WriteLine("");
            Console.WriteLine("========================================");
            Console.WriteLine(" API is running successfully!");
            Console.WriteLine("========================================");
            Console.WriteLine($" API URL: {url}");
            Console.WriteLine($" Swagger UI: {swaggerUrl}");
            Console.WriteLine("Opening Swagger UI in browser...");
            Console.WriteLine("========================================");
            Console.WriteLine("");
            
            // Open browser
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo { FileName = swaggerUrl, UseShellExecute = true };
                System.Diagnostics.Process.Start(psi);
                Console.WriteLine(" Browser opened successfully!");
            }
            catch (Exception)
            {
                Console.WriteLine($" Could not open browser automatically. Visit manually: {swaggerUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

// Minimal-API endpoint groups (catalog for Mobile + Web). Role-based authorization
// gates App vs Web callers within each group. See Endpoints/EndpointRegistration.cs.
app.MapApiEndpoints();

// Redirect root to swagger
app.MapGet("/", context =>
{
	context.Response.Redirect("/swagger");
	return Task.CompletedTask;
});

app.Run();

