using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.ProductDto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; } // unit, parking, storage, amenity
        public string Status { get; set; }
        public decimal BasePrice { get; set; }
        public int? SquareFeet { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? FloorNumber { get; set; }
        public string Description { get; set; }
        public List<ProductAmenityDto> Amenities { get; set; } = new();
        public string PrimaryPhoto { get; set; }
        public List<string> Photos { get; set; } = new();
        public Dictionary<string, string> Attributes { get; set; } = new();
        public UtilityMetersDto UtilityMeters { get; set; }
        public MaintenanceStatusDto MaintenanceStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductDetailDto : ProductDto
    {
        public List<ProductPhotoDetailDto> PhotoDetails { get; set; } = new();
        public List<string> VirtualTours { get; set; } = new();
        public List<MaintenanceRequestDto> MaintenanceRequests { get; set; } = new();
        public CurrentLeaseDto CurrentLease { get; set; }
    }

    public class ProductAmenityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal MonthlyFee { get; set; }
        public string Status { get; set; }
    }

    public class ProductPhotoDetailDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class UtilityMetersDto
    {
        public string WaterMeterId { get; set; }
        public string ElectricityMeterId { get; set; }
        public WaterMeterDto Water { get; set; }
        public ElectricityMeterDto Electricity { get; set; }
    }

    public class WaterMeterDto
    {
        public string MeterNumber { get; set; }
        public decimal LastReading { get; set; }
        public DateTime LastReadingDate { get; set; }
    }

    public class ElectricityMeterDto
    {
        public string MeterNumber { get; set; }
        public decimal LastReading { get; set; }
        public DateTime LastReadingDate { get; set; }
    }

    public class MaintenanceStatusDto
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public bool BlockLeasing { get; set; }
        public bool IsActive { get; set; }
    }

    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AssignedTo { get; set; }
    }

    public class CurrentLeaseDto
    {
        public string TenantName { get; set; }
        public DateTime LeaseStart { get; set; }
        public DateTime LeaseEnd { get; set; }
        public decimal MonthlyRent { get; set; }
    }

    public class PricingRuleDto
    {
        public int Id { get; set; }
        public int LeaseTermMonths { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal AdminFee { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class MeterReadingDto
    {
        public int Id { get; set; }
        public string MeterType { get; set; }
        public string MeterNumber { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal PreviousReading { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal UnitsConsumed { get; set; }
        public decimal RatePerUnit { get; set; }
        public decimal TotalCharge { get; set; }
    }
}
