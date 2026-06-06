using ApartmentManagementSystem.Application.Command.AnnouncementCommand;
using ApartmentManagementSystem.Application.Query.AnnouncementQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;


public static class AnnouncementEndpoints
{
    public static IEndpointRouteBuilder MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/announcements").WithTags("Announcements (Shared)");

        g.MapGet("/", async ([FromQuery] string? status, [FromQuery] string? audience,
                              [FromQuery] int? propertyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
                              IMediator mediator = null!) =>
        {
            var result = await mediator.Send(new GetAnnouncementsQuery
            {
                Status = status,
                Audience = audience,
                PropertyId = propertyId,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 20 : pageSize,
            });
            return ApiResponseExtensions.Ok(result, "Announcements retrieved");
        })
         .RequireAuthorization();

        g.MapPost("/", async ([FromBody] CreateAnnouncementCommand req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(req);
                return ApiResponseExtensions.Created(result, "Announcement created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAnnouncementByIdQuery { Id = id });
            return result is null
                ? ApiResponseExtensions.NotFound("Announcement not found")
                : ApiResponseExtensions.Ok(result, "Announcement retrieved");
        })
         .RequireAuthorization();

        g.MapPost("/{id:int}/send-now", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new SendAnnouncementNowCommand { AnnouncementId = id });
                return ApiResponseExtensions.Ok(result, "Announcement sent");
            }
            catch (KeyNotFoundException ex)
            {
                return ApiResponseExtensions.NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponseExtensions.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponseExtensions.Error(ex);
            }
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/{id:int}/deliveries", async (int id, IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        {
            try
            {
                var result = await mediator.Send(new GetAnnouncementDeliveriesQuery
                {
                    AnnouncementId = id,
                    Page = page <= 0 ? 1 : page,
                    PageSize = pageSize <= 0 ? 50 : pageSize,
                });
                return ApiResponseExtensions.Ok(result, "Deliveries retrieved");
            }
            catch (Exception ex)
            {
                return ApiResponseExtensions.Error(ex);
            }
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/cover/upload", async (HttpContext ctx, IMediator mediator) =>
        {
            try
            {
                var file = ctx.Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return ApiResponseExtensions.BadRequest("No file provided");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var result = await mediator.Send(new UploadAnnouncementCoverCommand
                {
                    FileData = ms.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                });
                return ApiResponseExtensions.Ok(result, "Cover uploaded");
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

        return app;
    }
}
