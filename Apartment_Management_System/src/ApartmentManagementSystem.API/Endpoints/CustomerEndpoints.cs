using ApartmentManagementSystem.Application.Command.CustomersCommand;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;
using ApartmentManagementSystem.Application.Query.CustomerQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/customers/* – tenant/owner CRUD, addresses, leases, documents, notes,
/// communications, move checklists, search, owner portfolio.
/// Replaces the deleted CustomersController. Every previous route is preserved.
/// </summary>
public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/customers")
                   .WithTags("Customers (Shared)")
                   .RequireAuthorization();

        // ===== Profile =======================================================
        g.MapGet("/{customerId:int}/profile", async (int customerId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetCustomerProfileQuery { CustomerId = customerId });
                return result is null
                    ? ApiResponseExtensions.NotFound("Customer not found")
                    : ApiResponseExtensions.Ok(result, "Customer profile retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/tenant", async ([FromBody] CreateCustomerRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new CreateCustomerCommand
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    CustomerType = request.CustomerType ?? "tenant",
                    DateOfBirth = request.DateOfBirth,
                    GovernmentId = request.GovernmentId,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone
                });
                return ApiResponseExtensions.Created(result, "Customer created successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPut("/{customerId:int}", async (int customerId, [FromBody] UpdateCustomerRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateCustomerCommand
                {
                    CustomerId = customerId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone
                });
                return result is null
                    ? ApiResponseExtensions.NotFound("Customer not found")
                    : ApiResponseExtensions.Ok(result, "Customer updated successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Addresses =====================================================
        g.MapGet("/{customerId:int}/addresses", async (int customerId, IMediator mediator) =>
        {
            try
            {
                var addresses = await mediator.Send(new GetCustomerAddressesQuery { CustomerId = customerId });
                return ApiResponseExtensions.Ok(addresses, "Addresses retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{customerId:int}/addresses", async (int customerId, [FromBody] AddressDto request, IMediator mediator) =>
        {
            try
            {
                var id = await mediator.Send(new AddAddressCommand
                {
                    CustomerId = customerId,
                    AddressType = request.AddressType,
                    Street = request.Street,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    IsPrimary = request.IsPrimary
                });
                return ApiResponseExtensions.Created(id, "Address added successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPut("/addresses/{addressId:int}", async (int addressId, [FromBody] AddressDto request, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new UpdateAddressCommand
                {
                    AddressId = addressId,
                    AddressType = request.AddressType,
                    Street = request.Street,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    IsPrimary = request.IsPrimary
                });
                return ok
                    ? ApiResponseExtensions.Ok(true, "Address updated successfully")
                    : ApiResponseExtensions.NotFound("Address not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPatch("/addresses/{addressId:int}/set-primary", async (int addressId, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new SetPrimaryAddressCommand { AddressId = addressId });
                return ok
                    ? ApiResponseExtensions.Ok(true, "Address set as primary")
                    : ApiResponseExtensions.NotFound("Address not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapDelete("/addresses/{addressId:int}", async (int addressId, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new DeleteAddressCommand { AddressId = addressId });
                return ok
                    ? ApiResponseExtensions.Ok(true, "Address deleted successfully")
                    : ApiResponseExtensions.NotFound("Address not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Leases ========================================================
        g.MapGet("/{customerId:int}/leases/history", async (int customerId, IMediator mediator) =>
        {
            try
            {
                var leases = await mediator.Send(new GetCustomerLeasesQuery { CustomerId = customerId });
                return ApiResponseExtensions.Ok(leases, "Leases retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Lease documents ===============================================
        g.MapGet("/leases/{leaseId:int}/documents", async (int leaseId, IMediator mediator) =>
        {
            try
            {
                var docs = await mediator.Send(new GetLeaseDocumentsQuery { LeaseId = leaseId });
                return ApiResponseExtensions.Ok(docs, "Documents retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/leases/{leaseId:int}/documents",
            async (int leaseId, [FromForm] UploadLeaseDocumentRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UploadLeaseDocumentCommand
                {
                    LeaseId = leaseId,
                    DocumentType = request.DocumentType,
                    FileName = request.File?.FileName ?? "document",
                    FileUrl = request.FileUrl,
                    Description = request.Description
                });
                return ApiResponseExtensions.Created(result, "Document uploaded successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).DisableAntiforgery();

        g.MapPost("/leases/{leaseId:int}/sign", async (int leaseId, [FromBody] SignLeaseRequest request, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new SignLeaseCommand { LeaseId = leaseId, SignedBy = request.SignedBy });
                return ApiResponseExtensions.Ok(ok, "Lease signed successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Notes =========================================================
        g.MapGet("/{customerId:int}/notes", async (int customerId, IMediator mediator) =>
        {
            try
            {
                var notes = await mediator.Send(new GetCustomerNotesQuery { CustomerId = customerId });
                return ApiResponseExtensions.Ok(notes, "Notes retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{customerId:int}/notes", async (int customerId, [FromBody] AddNoteRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new AddCustomerNoteCommand
                {
                    CustomerId = customerId,
                    Content = request.Content,
                    CreatedBy = user.Identity?.Name ?? "System"
                });
                return ApiResponseExtensions.Created(result, "Note added successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPut("/notes/{noteId:int}", async (int noteId, [FromBody] AddNoteRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateCustomerNoteCommand { NoteId = noteId, Content = request.Content });
                return ApiResponseExtensions.Ok(result, "Note updated successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapDelete("/notes/{noteId:int}", async (int noteId, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new DeleteCustomerNoteCommand { NoteId = noteId });
                return ApiResponseExtensions.Ok(ok, "Note deleted successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Communications ================================================
        g.MapGet("/{customerId:int}/communications", async (int customerId,
            [FromQuery] int pageNumber, [FromQuery] int pageSize,
            [FromQuery] string? type, [FromQuery] string? direction,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetCommunicationsQuery
                {
                    CustomerId = customerId,
                    PageNumber = pageNumber > 0 ? pageNumber : 1,
                    PageSize = pageSize > 0 ? pageSize : 20,
                    Type = type ?? string.Empty,
                    Direction = direction ?? string.Empty
                });
                return ApiResponseExtensions.Ok(result, "Communications retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{customerId:int}/communications",
            async (int customerId, [FromBody] SendCommunicationRequest request, IMediator mediator) =>
        {
            try
            {
                var id = await mediator.Send(new SendCommunicationCommand
                {
                    CustomerId = customerId,
                    Type = request.Type,
                    Subject = request.Subject,
                    Message = request.Message,
                    Direction = "outbound"
                });
                return ApiResponseExtensions.Created(id, "Communication sent successfully");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Move-in / out checklists =====================================
        g.MapPost("/leases/{leaseId:int}/move-in-checklist",
            async (int leaseId, [FromBody] CreateMoveChecklistRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new CreateMoveChecklistCommand
                {
                    LeaseId = leaseId,
                    ChecklistType = "move_in",
                    InspectionDate = request.InspectionDate,
                    InspectorName = request.InspectorName,
                    OverallCondition = request.OverallCondition,
                    Notes = request.Notes
                });
                return ApiResponseExtensions.Created(result, "Move-in checklist created successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/checklists/{checklistId:int}", async (int checklistId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetMoveChecklistQuery { ChecklistId = checklistId });
                return result is null
                    ? ApiResponseExtensions.NotFound("Checklist not found")
                    : ApiResponseExtensions.Ok(result, "Move checklist retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // ===== Search + listing =============================================
        g.MapGet("/search", async ([FromQuery] string? q, IMediator mediator) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return ApiResponseExtensions.BadRequest("Search query cannot be empty");

                var result = await mediator.Send(new SearchCustomersQuery { SearchTerm = q });
                return ApiResponseExtensions.Ok(result, "Search results retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/tenants", async ([FromQuery] int page, [FromQuery] int limit, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetAllCustomersQuery
                {
                    PageNumber = page > 0 ? page : 1,
                    PageSize = limit > 0 ? limit : 20
                });
                return ApiResponseExtensions.Ok(result, "Customers retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/properties/{propertyId:int}/tenants/active", async (int propertyId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetPropertyTenantsQuery { PropertyId = propertyId });
                return ApiResponseExtensions.Ok(result, "Active tenants retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/owners/{ownerId:int}/portfolio", async (int ownerId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetOwnerPortfolioQuery { OwnerId = ownerId });
                return result is null
                    ? ApiResponseExtensions.NotFound("Owner not found")
                    : ApiResponseExtensions.Ok(result, "Portfolio retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        return app;
    }
}
