using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.ProductDto;

namespace ApartmentManagementSystem.Application.DTOs.ProductDto
{
    // Request DTOs for API
    public class CreateProductRequest
    {
        public string ProductType { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int? SquareFeet { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? FloorNumber { get; set; }
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int? SquareFeet { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? FloorNumber { get; set; }
    }

    public class UpdateProductStatusRequest
    {
        public string Status { get; set; }
    }

    public class UpdateMaintenanceStatusRequest
    {
        public string MaintenanceStatus { get; set; }
        public string Reason { get; set; }
        public System.DateTime EstimatedCompletionDate { get; set; }
        public bool BlockLeasing { get; set; }
        public string Notes { get; set; }
    }

    public class CreatePricingRulesRequest
    {
        public List<PricingRuleInputDto> PricingRules { get; set; } = new();
    }

    public class PricingRuleInputDto
    {
        public int LeaseTermMonths { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal AdminFee { get; set; }
        public System.DateTime EffectiveFrom { get; set; }
        public System.DateTime? EffectiveTo { get; set; }
    }

    public class MeterReadingInputDto
    {
        public string MeterType { get; set; }
        public string MeterNumber { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal PreviousReading { get; set; }
        public System.DateTime ReadingDate { get; set; }
        public decimal UnitsConsumed { get; set; }
        public decimal RatePerUnit { get; set; }
        public decimal TotalCharge { get; set; }
    }

    public class RecordMeterReadingsRequest
    {
        public List<MeterReadingInputDto> Readings { get; set; } = new();
    }

    public class AssignAmenityRequest
    {
        public int AmenityId { get; set; }
        public string Name { get; set; }
        public decimal MonthlyFee { get; set; }
        public string AssignedSpot { get; set; }
        public System.DateTime EffectiveDate { get; set; }
    }

    public class AssignAmenitiesToUnitRequest
    {
        public List<AssignAmenityRequest> Amenities { get; set; } = new();
    }

    public class BulkUpdateProductStatusRequest
    {
        public List<int> ProductIds { get; set; } = new();
        public string Status { get; set; }
        public string Reason { get; set; }
        public MaintenanceScheduleDto MaintenanceSchedule { get; set; }
    }

    public class MaintenanceScheduleDto
    {
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    }

    // Pagination Request
    public class GetProductsFilterRequest
    {
        public string Status { get; set; }
        public string ProductType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Bedrooms { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetAvailableUnitsFilterRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? MinSqft { get; set; }
        public int? MaxSqft { get; set; }
        public System.DateTime? MoveInDate { get; set; }
        public int? FloorNumber { get; set; }
        public string AmenitiesIncluded { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Response Wrapper
    public class PaginatedProductResponse<T>
    {
        public List<T> Products { get; set; } = new();
        public PaginationInfoDto Pagination { get; set; }
    }

    public class PaginationInfoDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
}
