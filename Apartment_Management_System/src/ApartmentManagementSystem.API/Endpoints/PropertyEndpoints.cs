using ApartmentManagementSystem.Application.Command.PropertyCommand;
using ApartmentManagementSystem.Application.DTOs.PropertyDto;
using ApartmentManagementSystem.Application.Query.PropertyQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

public static class PropertyEndpoints
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/properties")
                   .WithTags("Properties (Web)")
                   .RequireAuthorization(p => p.RequireRole(
                       AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/", async ([FromQuery] string? status, [FromQuery] string? q,
                             [FromQuery] int page, [FromQuery] int pageSize,
                             IMediator mediator) =>
        {
            try
            {
                var rows = await mediator.Send(new GetPropertiesQuery
                {
                    Status = status,
                    Q = q,
                    Page = page <= 0 ? 1 : page,
                    PageSize = pageSize <= 0 ? 50 : pageSize,
                });
                return ApiResponseExtensions.Ok(rows, "Properties retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/", async ([FromBody] CreatePropertyRequest req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new CreatePropertyCommand
                {
                    Code = req.Code,
                    Name = req.Name,
                    Description = req.Description,
                    AddressLine = req.AddressLine,
                    City = req.City,
                    State = req.State,
                    PostalCode = req.PostalCode,
                    Country = req.Country,
                    Latitude = req.Latitude,
                    Longitude = req.Longitude,
                    TotalUnits = req.TotalUnits,
                });
                return ApiResponseExtensions.Created(result, "Property created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return ApiResponseExtensions.Conflict(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                var p = await mediator.Send(new GetPropertyByIdQuery { Id = id });
                return p is null
                    ? ApiResponseExtensions.NotFound("Property not found")
                    : ApiResponseExtensions.Ok(p, "Property retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPut("/{id:int}", async (int id, [FromBody] CreatePropertyRequest req, IMediator mediator) =>
        {
            try
            {
                var updated = await mediator.Send(new UpdatePropertyCommand
                {
                    Id = id,
                    Name = req.Name,
                    Description = req.Description,
                    AddressLine = req.AddressLine,
                    City = req.City,
                    State = req.State,
                    PostalCode = req.PostalCode,
                    Country = req.Country,
                    TotalUnits = req.TotalUnits,
                });
                return updated is null
                    ? ApiResponseExtensions.NotFound("Property not found")
                    : ApiResponseExtensions.Ok(updated, "Property updated");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                var deleted = await mediator.Send(new DeletePropertyCommand { Id = id });
                return deleted
                    ? ApiResponseExtensions.Ok(new { id }, "Property archived")
                    : ApiResponseExtensions.NotFound("Property not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}/units", async (int id, IMediator mediator) =>
        {
            try
            {
                var rows = await mediator.Send(new GetPropertyUnitsQuery { PropertyId = id });
                return ApiResponseExtensions.Ok(rows, "Units retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}/photos", async (int id, IMediator mediator) =>
        {
            try
            {
                var rows = await mediator.Send(new GetPropertyPhotosQuery { PropertyId = id });
                return ApiResponseExtensions.Ok(rows, "Photos retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{id:int}/photos", (int id) =>
            EndpointResults.NotImplemented("UploadPropertyPhotoCommand"));

        g.MapGet("/{id:int}/owners", async (int id, IMediator mediator) =>
        {
            try
            {
                var rows = await mediator.Send(new GetPropertyOwnersQuery { PropertyId = id });
                return ApiResponseExtensions.Ok(rows, "Owners retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPost("/{id:int}/owners", (int id) =>
            EndpointResults.NotImplemented("AddPropertyOwnerCommand"));

        g.MapGet("/{id:int}/kpis", async (int id, IMediator mediator) =>
        {
            try
            {
                var k = await mediator.Send(new GetPropertyKpisQuery { Id = id });
                return ApiResponseExtensions.Ok(k, "KPIs retrieved");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        return app;
    }
}
