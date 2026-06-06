using ApartmentManagementSystem.Application.Command.CustomersCommand;
using ApartmentManagementSystem.Application.Query.CustomerQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;


public static class LeaseEndpoints
{
    public static IEndpointRouteBuilder MapLeaseEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/leases").WithTags("Leases (Shared)");

        // PM list — wraps the result in ApiResponse<T> envelope so the web
        // and mobile clients (which always read `env.data`) can parse it.
        g.MapGet("/", async (
            [FromQuery] string? status,
            [FromQuery] int? propertyId,
            [FromQuery] string? q,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetLeasesQuery
                {
                    Status = status,
                    PropertyId = propertyId,
                    SearchQuery = q,
                    PageNumber = page is > 0 ? page.Value : 1,
                    PageSize = pageSize is > 0 ? pageSize.Value : 20
                });
                return ApiResponseExtensions.Ok(result, "Leases retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/", async ([FromBody] CreateLeaseCommand req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(req);
                return ApiResponseExtensions.Created(result, "Lease created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        // Tenant + PM detail (wires when slice exists; for now stub)
        g.MapGet("/{id:int}", (int id) =>
            EndpointResults.NotImplemented("GetLeaseByIdQuery (top-level; today only customer-scoped exists)"))
         .RequireAuthorization();

        g.MapPatch("/{id:int}/status", (int id) =>
            EndpointResults.NotImplemented("UpdateLeaseStatusCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/{id:int}/send-for-signature", (int id) =>
            EndpointResults.NotImplemented("SendLeaseForSignatureCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/{id:int}/sign", async (int id, [FromBody] SignLeasePayload body, HttpContext ctx, IMediator mediator) =>
        {
            var signedBy = ctx.User.FindFirst("sub")?.Value
                           ?? ctx.User.FindFirst("userId")?.Value
                           ?? body.Signature
                           ?? "unknown";
            var ok = await mediator.Send(new SignLeaseCommand { LeaseId = id, SignedBy = signedBy });
            return ok ? Results.Ok(new { leaseId = id, status = "signed" }) : Results.NotFound();
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner, AppRoles.PropertyManager));

        g.MapPost("/{id:int}/renew", (int id) =>
            EndpointResults.NotImplemented("RenewLeaseCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/{id:int}/terminate", (int id) =>
            EndpointResults.NotImplemented("TerminateLeaseCommand"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        // Lease documents – wire existing handlers
        g.MapGet("/{id:int}/documents", async (int id, IMediator mediator) =>
        {
            try
            {
                var docs = await mediator.Send(new GetLeaseDocumentsQuery { LeaseId = id });
                return ApiResponseExtensions.Ok(docs, "Lease documents retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).RequireAuthorization();

        g.MapPost("/{id:int}/documents", (int id) =>
            EndpointResults.NotImplemented("UploadLeaseDocumentCommand (wire multipart-form upload)"))
         .RequireAuthorization();

        return app;
    }

    public sealed record SignLeasePayload(string? Signature, string? Method, DateTime? SignedAt);
}
