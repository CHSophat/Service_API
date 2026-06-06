using ApartmentManagementSystem.Application.Command.ProductCommand;
using ApartmentManagementSystem.Application.DTOs.ProductDto;
using ApartmentManagementSystem.Application.Query.ProductQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ApartmentManagementSystem.API.Endpoints;

/// <summary>
/// /api/v1/products/* – units, parking, storage, amenities. Catalog browse +
/// search is anonymous; create/update/delete/status/maintenance/photos require
/// Admin or Manager. Replaces the deleted ProductsController.
/// </summary>
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup($"{EndpointRegistration.ApiBase}/products").WithTags("Products (Shared)");

        // ===== Catalog browse (anonymous) ====================================
        g.MapGet("/units", async ([FromQuery] string? status, [FromQuery] string? productType,
                                   [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
                                   [FromQuery] short? bedrooms,
                                //    [FromQuery] int pageNumber, [FromQuery] int pageSize,
                                   IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetAllProductsQuery
                {
                    Status = status ?? string.Empty,
                    ProductType = productType ?? string.Empty,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Bedrooms = bedrooms,
                    // PageNumber = pageNumber > 0 ? pageNumber : 1,
                    // PageSize = pageSize > 0 ? pageSize : 20
                });
                return ApiResponseExtensions.Ok(result, "Units retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapGet("/units/available", async ([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
                                              [FromQuery] short? bedrooms, [FromQuery] decimal? bathrooms,
                                              [FromQuery] int? minSqft, [FromQuery] int? maxSqft,
                                              [FromQuery] DateTime? moveInDate, [FromQuery] int? floorNumber,
                                              [FromQuery] string? amenitiesIncluded,
                                              [FromQuery] int pageNumber, [FromQuery] int pageSize,
                                              IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetAvailableUnitsQuery
                {
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Bedrooms = bedrooms,
                    Bathrooms = bathrooms,
                    MinSqft = minSqft,
                    MaxSqft = maxSqft,
                    MoveInDate = moveInDate,
                    FloorNumber = floorNumber,
                    AmenitiesIncluded = amenitiesIncluded ?? string.Empty,
                    PageNumber = pageNumber > 0 ? pageNumber : 1,
                    PageSize = pageSize > 0 ? pageSize : 20
                });
                return ApiResponseExtensions.Ok(result, "Available units found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        g.MapGet("/{productId:int}", async (int productId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetProductByIdQuery { ProductId = productId });
                return result is null
                    ? ApiResponseExtensions.NotFound("Product not found")
                    : ApiResponseExtensions.Ok(result, "Product details retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        // ===== Catalog gaps from API catalog (App + Web specific) ============
        g.MapGet("/{id:int}/payment-breakdown", (int id) =>
            EndpointResults.NotImplemented("GetUnitPaymentBreakdownQuery (rent + utilities + fees)"))
         .RequireAuthorization(p => p.RequireRole(AppRoles.Tenant, AppRoles.Owner));

        g.MapGet("/by-property", ([FromQuery] int propertyId) =>
            EndpointResults.NotImplemented("GetProductsByPropertyQuery"))
         .RequireAuthorization(p => p.RequireRole(
             AppRoles.PropertyManager, AppRoles.Admin, AppRoles.Staff));

        // ===== Search (anonymous) ============================================
        g.MapGet("/search", async ([FromQuery] string? searchTerm, [FromQuery] string? productType,
                                    [FromQuery] string? status, [FromQuery] decimal? minPrice,
                                    [FromQuery] decimal? maxPrice, [FromQuery] short? bedrooms,
                                    [FromQuery] int pageNumber, [FromQuery] int pageSize,
                                    IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new SearchProductsQuery
                {
                    SearchTerm = searchTerm ?? string.Empty,
                    ProductType = productType ?? string.Empty,
                    Status = status ?? string.Empty,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Bedrooms = bedrooms,
                    PageNumber = pageNumber > 0 ? pageNumber : 1,
                    PageSize = pageSize > 0 ? pageSize : 20
                });
                return ApiResponseExtensions.Ok(result, "Search results retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        // ===== Mutations (Admin/Manager) =====================================
        var pm = g.MapGroup("/").RequireAuthorization(p => p.RequireRole("Admin", "Manager"));

        pm.MapPost("/", async ([FromBody] CreateProductRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new CreateProductCommand
                {
                    ProductType = request.ProductType,
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    BasePrice = request.BasePrice,
                    SquareFeet = request.SquareFeet,
                    Bedrooms = request.Bedrooms,
                    Bathrooms = request.Bathrooms,
                    FloorNumber = request.FloorNumber
                });
                return ApiResponseExtensions.Created(result, "Product created successfully");
            }
            catch (ArgumentException ex) { return ApiResponseExtensions.BadRequest(ex.Message); }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapPut("/{productId:int}", async (int productId, [FromBody] UpdateProductRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateProductCommand
                {
                    ProductId = productId,
                    Name = request.Name,
                    Description = request.Description,
                    BasePrice = request.BasePrice,
                    SquareFeet = request.SquareFeet,
                    Bedrooms = request.Bedrooms,
                    Bathrooms = request.Bathrooms,
                    FloorNumber = request.FloorNumber
                });
                return ApiResponseExtensions.Ok(result, "Product updated successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapPatch("/{productId:int}/status",
            async (int productId, [FromBody] UpdateProductStatusRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateProductStatusCommand
                {
                    ProductId = productId,
                    Status = request.Status
                });
                return ApiResponseExtensions.Ok(result, "Product status updated successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapDelete("/{productId:int}", async (int productId, IMediator mediator) =>
        {
            try
            {
                var ok = await mediator.Send(new DeleteProductCommand { ProductId = productId });
                return ok
                    ? ApiResponseExtensions.Ok(new { }, "Product deleted successfully")
                    : ApiResponseExtensions.NotFound("Product not found");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapPatch("/{productId:int}/maintenance",
            async (int productId, [FromBody] UpdateMaintenanceStatusRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateMaintenanceStatusCommand
                {
                    ProductId = productId,
                    MaintenanceStatus = request.MaintenanceStatus,
                    Reason = request.Reason,
                    EstimatedCompletionDate = request.EstimatedCompletionDate,
                    BlockLeasing = request.BlockLeasing,
                    Notes = request.Notes
                });
                return ApiResponseExtensions.Ok(result, "Unit marked as under maintenance");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapGet("/{productId:int}/maintenance-requests",
            async (int productId, [FromQuery] string? status, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetProductMaintenanceRequestsQuery
                {
                    ProductId = productId,
                    Status = status
                });
                return ApiResponseExtensions.Ok(result, "Maintenance requests retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapPost("/{productId:int}/photos",
            async (int productId, [FromBody] List<string>? photoUrls, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new AddProductPhotosCommand
                {
                    ProductId = productId,
                    PhotoUrls = photoUrls ?? new List<string>()
                });
                return ApiResponseExtensions.Created(result, "Photos uploaded successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        pm.MapPatch("/bulk-status",
            async ([FromBody] BulkUpdateProductStatusRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new BulkUpdateProductStatusCommand
                {
                    ProductIds = request.ProductIds ?? new List<int>(),
                    Status = request.Status,
                    Reason = request.Reason,
                    MaintenanceSchedule = request.MaintenanceSchedule
                });
                var msg = result.ContainsKey("updated_count")
                    ? $"{result["updated_count"]} units updated successfully"
                    : "Units updated";
                return ApiResponseExtensions.Ok(result, msg);
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        });

        // Photos GET is anonymous (already public per old controller)
        g.MapGet("/{productId:int}/photos", async (int productId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetProductPhotosQuery { ProductId = productId });
                return ApiResponseExtensions.Ok(result, "Photos retrieved successfully");
            }
            catch (Exception ex) { return ApiResponseExtensions.Error(ex); }
        }).AllowAnonymous();

        return app;
    }
}
