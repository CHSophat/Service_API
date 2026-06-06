using ApartmentManagementSystem.Application.Command.ListingCommand;
using ApartmentManagementSystem.Application.Query.ListingQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;


public static class ListingEndpoints
{
    public static IEndpointRouteBuilder MapListingEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/listings")
                   .WithTags("Listings (Web)")
                   .RequireAuthorization(p => p.RequireRole(
                       AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        g.MapGet("/", async ([FromQuery] string? status, [FromQuery] bool? featured, [FromQuery] string? order,
                              [FromQuery] int page, [FromQuery] int pageSize, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetListingsQuery
            {
                Status = status,
                Featured = featured,
                Order = order,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 20 : pageSize,
            });
            return ApiResponseExtensions.Ok(result, "Listings retrieved");
        });

        g.MapPost("/", async ([FromBody] CreateListingCommand req, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(req);
                return ApiResponseExtensions.Created(result, "Listing created");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetListingByIdQuery { Id = id });
            return result is null
                ? ApiResponseExtensions.NotFound("Listing not found")
                : ApiResponseExtensions.Ok(result, "Listing retrieved");
        });

        g.MapPut("/{id:int}", async (int id, [FromBody] UpdateListingCommand req, IMediator mediator) =>
        {
            try
            {
                req.Id = id;
                var result = await mediator.Send(req);
                return result is null
                    ? ApiResponseExtensions.NotFound("Listing not found")
                    : ApiResponseExtensions.Ok(result, "Listing updated");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new DeleteListingCommand { Id = id });
                return ok
                    ? ApiResponseExtensions.Ok(new { id }, "Listing archived")
                    : ApiResponseExtensions.NotFound("Listing not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPatch("/{id:int}/publish", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new PublishListingCommand { Id = id });
                return result is null
                    ? ApiResponseExtensions.NotFound("Listing not found")
                    : ApiResponseExtensions.Ok(result, "Listing published");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPatch("/{id:int}/unpublish", async (int id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UnpublishListingCommand { Id = id });
                return result is null
                    ? ApiResponseExtensions.NotFound("Listing not found")
                    : ApiResponseExtensions.Ok(result, "Listing unpublished");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        g.MapPatch("/{id:int}/feature", async (int id, [FromBody] SetFeaturedListingCommand req, IMediator mediator) =>
        {
            try
            {
                req.Id = id;
                var result = await mediator.Send(req);
                return result is null
                    ? ApiResponseExtensions.NotFound("Listing not found")
                    : ApiResponseExtensions.Ok(result, req.IsFeatured ? "Listing featured" : "Listing unfeatured");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        return app;
    }
}
