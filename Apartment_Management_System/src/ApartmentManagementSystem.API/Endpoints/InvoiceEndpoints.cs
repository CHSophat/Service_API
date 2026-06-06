using ApartmentManagementSystem.Application.Command.InvoiceCommand;
using ApartmentManagementSystem.Application.Query.InvoiceQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;


public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/invoices").WithTags("Invoices (Shared)");

        // Web list
        g.MapGet("/", async ([FromQuery] string? status, [FromQuery] int? propertyId,
                               [FromQuery] string? period, [FromQuery] string? q,
                               [FromQuery] int page, [FromQuery] int pageSize, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoicesQuery
            {
                Status = status,
                PropertyId = propertyId,
                Period = period,
                Q = q,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 20 : pageSize,
            });
            return ApiResponseExtensions.Ok(result, "Invoices retrieved");
        })
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapPost("/", async ([FromBody] CreateInvoiceCommand req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(req);
                return ApiResponseExtensions.Created(result, "Invoice created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        // Tenant + Web detail
        g.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceByIdQuery { Id = id });
            return result is null
                ? ApiResponseExtensions.NotFound("Invoice not found")
                : ApiResponseExtensions.Ok(result, "Invoice retrieved");
        })
         .RequireAuthorization();

        g.MapPut("/{id:int}", async (int id, [FromBody] UpdateInvoiceCommand req, IMediator mediator) =>
        {
            try
            {
                req.Id = id;
                var result = await mediator.Send(req);
                return result is null
                    ? ApiResponseExtensions.NotFound("Invoice not found")
                    : ApiResponseExtensions.Ok(result, "Invoice updated");
            }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new DeleteInvoiceCommand { Id = id });
                return ok
                    ? ApiResponseExtensions.Ok(new { id }, "Invoice deleted")
                    : ApiResponseExtensions.NotFound("Invoice not found");
            }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/{id:int}/send", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new SendInvoiceCommand { Id = id });
                return result is null
                    ? ApiResponseExtensions.NotFound("Invoice not found")
                    : ApiResponseExtensions.Ok(result, "Invoice marked as sent");
            }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapPost("/{id:int}/void", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new VoidInvoiceCommand { Id = id });
                return result is null
                    ? ApiResponseExtensions.NotFound("Invoice not found")
                    : ApiResponseExtensions.Ok(result, "Invoice voided");
            }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization(p => p.RequireRole(AppRoles.PropertyManager, AppRoles.Admin));

        g.MapGet("/{id:int}/pdf", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetInvoicePdfQuery { Id = id });
                return result is null
                    ? ApiResponseExtensions.NotFound("Invoice not found")
                    : Results.File(result.Bytes, "application/pdf", result.FileName);
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         .RequireAuthorization();

        g.MapGet("/outstanding", async ([FromQuery] int customerId, IMediator mediator) =>
        {
            try
            {
                var rows = await mediator.Send(new GetOutstandingInvoicesQuery { CustomerId = customerId });
                return ApiResponseExtensions.Ok(rows, "Outstanding invoices retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        })
         // Tenants/Owners need it for their own portal; PM/Admin/Staff need it
         // to look up balances for any customer they manage from the MSI app.
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.Tenant, AppRoles.Owner,
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        return app;
    }
}
