using ApartmentManagementSystem.Application.Command.UploadCommand;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/uploads/* – shared file ingestion. Per catalog: profile photo +
/// maintenance photo are App-only, property photo is Web-only, document is shared.
/// </summary>
public static class UploadEndpoints
{
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/uploads").WithTags("Uploads (Shared)");

        g.MapPost("/profile-photo", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                int.TryParse(ctx.Request.Form["customerId"], out int customerId);
                var result = await mediator.Send(new UploadProfilePhotoCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    CustomerId = customerId > 0 ? customerId : null
                });
                return ApiResponseExtensions.Ok(result, "Profile photo uploaded");
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
         .DisableAntiforgery()
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapPost("/maintenance-photo", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                int.TryParse(ctx.Request.Form["requestId"], out int requestId);
                var result = await mediator.Send(new UploadMaintenancePhotoCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    MaintenanceRequestId = requestId > 0 ? requestId : null
                });
                return ApiResponseExtensions.Ok(result, "Maintenance photo uploaded");
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
         .DisableAntiforgery()
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapPost("/property-photo", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                int.TryParse(ctx.Request.Form["propertyId"], out int propertyId);
                var result = await mediator.Send(new UploadPropertyPhotoCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    PropertyId = propertyId > 0 ? propertyId : null
                });
                return ApiResponseExtensions.Ok(result, "Property photo uploaded");
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
         .DisableAntiforgery()
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/document", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var category = ctx.Request.Form["category"].ToString();
                var result = await mediator.Send(new UploadDocumentCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Category = category
                });
                return ApiResponseExtensions.Ok(result, "Document uploaded");
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
         .DisableAntiforgery()
         .RequireAuthorization();

        return app;
    }
}
