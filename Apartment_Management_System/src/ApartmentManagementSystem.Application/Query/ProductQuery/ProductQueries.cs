using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.ProductDto;

namespace ApartmentManagementSystem.Application.Query.ProductQuery
{
    // Get all products with pagination
    public class GetAllProductsQuery : IRequest<PaginatedProductResponse<ProductDto>>
    {
        public required string Status { get; set; }
        public required string ProductType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Bedrooms { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get available units for leasing
    public class GetAvailableUnitsQuery : IRequest<PaginatedProductResponse<ProductDto>>
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? MinSqft { get; set; }
        public int? MaxSqft { get; set; }
        public DateTime? MoveInDate { get; set; }
        public int? FloorNumber { get; set; }
        public required string AmenitiesIncluded { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get single product details
    public class GetProductByIdQuery : IRequest<ProductDetailDto>
    {
        public int ProductId { get; set; }
    }

    // Get products by type (units, parking, storage, amenities)
    public class GetProductsByTypeQuery : IRequest<List<ProductDto>>
    {
        public required string ProductType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get products by status (vacant, occupied, maintenance, unavailable)
    public class GetProductsByStatusQuery : IRequest<List<ProductDto>>
    {
        public required string Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get all amenities available for assignment
    public class GetAvailableAmenitiesQuery : IRequest<List<ProductDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get product photos
    public class GetProductPhotosQuery : IRequest<List<ProductPhotoDetailDto>>
    {
        public int ProductId { get; set; }
    }

    // Get product attributes
    public class GetProductAttributesQuery : IRequest<Dictionary<string, string>>
    {
        public int ProductId { get; set; }
    }

    // Get maintenance requests for a product
    public class GetProductMaintenanceRequestsQuery : IRequest<List<MaintenanceRequestDto>>
    {
        public int ProductId { get; set; }
        public string Status { get; set; } // optional filter
    }

    // Get pricing rules for a product
    public class GetProductPricingRulesQuery : IRequest<List<PricingRuleDto>>
    {
        public int ProductId { get; set; }
    }

    // Get meter readings for a product
    public class GetProductMeterReadingsQuery : IRequest<List<MeterReadingDto>>
    {
        public int ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // Search products by criteria
    public class SearchProductsQuery : IRequest<PaginatedProductResponse<ProductDto>>
    {
        public required string SearchTerm { get; set; }
        public required string ProductType { get; set; }
        public required string Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Bedrooms { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get products under maintenance
    public class GetProductsUnderMaintenanceQuery : IRequest<List<ProductDetailDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Get product count by status
    public class GetProductCountByStatusQuery : IRequest<Dictionary<string, int>>
    {
    }

    // Get amenities assigned to a unit
    public class GetUnitAmenitiesQuery : IRequest<List<ProductAmenityDto>>
    {
        public int ProductId { get; set; }
    }
}
