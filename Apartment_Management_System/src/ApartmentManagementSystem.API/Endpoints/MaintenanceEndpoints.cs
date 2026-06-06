using ApartmentManagementSystem.Application.Command.MaintenanceCommand;
using ApartmentManagementSystem.Application.Query.MaintenanceQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;


public static class MaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/maintenance").WithTags("Maintenance (Shared)");

        // Web kanban list
        g.MapGet("/requests", async ([FromQuery] string? status, [FromQuery] string? priority,
                                       [FromQuery] int? propertyId, [FromQuery] int page, [FromQuery] int pageSize,
                                       IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMaintenanceRequestsQuery
            {
                Status = status,
                Priority = priority,
                ProductId = propertyId,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 20 : pageSize,
            });
            return ApiResponseExtensions.Ok(result, "Maintenance requests retrieved");
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        // Tenant create (already supported via /products/{id}/maintenance-requests in controller;
        // accept the catalog shape here too for client symmetry)
        g.MapPost("/requests", async ([FromBody] CreateMaintenanceRequestCommand req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(req);
                return ApiResponseExtensions.Created(result, "Maintenance request created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization();

        g.MapGet("/requests/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMaintenanceRequestByIdQuery { Id = id });
            return result is null
                ? ApiResponseExtensions.NotFound("Maintenance request not found")
                : ApiResponseExtensions.Ok(result, "Maintenance request retrieved");
        })
         .RequireAuthorization();

        g.MapPatch("/requests/{id:int}/status", (int id) =>
            EndpointResults.NotImplemented("UpdateMaintenanceStatusCommand"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPatch("/requests/{id:int}/assign", (int id) =>
            EndpointResults.NotImplemented("AssignMaintenanceCommand"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/requests/{id:int}/photos", (int id) =>
            EndpointResults.NotImplemented("UploadMaintenancePhotoCommand (Tenant)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        // Vendors (Web)
        g.MapGet("/vendors", () =>
            EndpointResults.NotImplemented("GetVendorsQuery"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/vendors", () =>
            EndpointResults.NotImplemented("CreateVendorCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapGet("/sla-summary", () =>
            EndpointResults.NotImplemented("GetMaintenanceSlaSummaryQuery"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        return app;
    }
}
