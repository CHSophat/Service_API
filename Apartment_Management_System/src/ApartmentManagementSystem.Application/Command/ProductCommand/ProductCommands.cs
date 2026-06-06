using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.ProductDto;

namespace ApartmentManagementSystem.Application.Command.ProductCommand
{
    // Create new product (unit, parking, storage, or amenity)
    public class CreateProductCommand : IRequest<ProductDto>
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

    // Update existing product
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int? SquareFeet { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? FloorNumber { get; set; }
    }

    // Update product status
    public class UpdateProductStatusCommand : IRequest<ProductDto>
    {
        public int ProductId { get; set; }
        public string Status { get; set; }
    }

    // Delete product
    public class DeleteProductCommand : IRequest<bool>
    {
        public int ProductId { get; set; }
    }

    // Update maintenance status
    public class UpdateMaintenanceStatusCommand : IRequest<ProductDetailDto>
    {
        public int ProductId { get; set; }
        public string MaintenanceStatus { get; set; }
        public string Reason { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public bool BlockLeasing { get; set; }
        public string Notes { get; set; }
    }

    // Create pricing rules
    public class CreatePricingRulesCommand : IRequest<List<PricingRuleDto>>
    {
        public int ProductId { get; set; }
        public List<PricingRuleInputDto> PricingRules { get; set; } = new();
    }

    // Record meter readings
    public class RecordMeterReadingsCommand : IRequest<Dictionary<string, object>>
    {
        public int ProductId { get; set; }
        public List<MeterReadingInputDto> Readings { get; set; } = new();
    }

    // Assign amenities to unit
    public class AssignAmenitiesToUnitCommand : IRequest<ProductDetailDto>
    {
        public int UnitProductId { get; set; }
        public List<AssignAmenityRequest> Amenities { get; set; } = new();
    }

    // Add photos to product
    public class AddProductPhotosCommand : IRequest<List<ProductPhotoDetailDto>>
    {
        public int ProductId { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
        public int PrimaryPhotoIndex { get; set; } = 0;
        public string VirtualTourUrl { get; set; }
    }

    // Add product attributes
    public class AddProductAttributesCommand : IRequest<Dictionary<string, string>>
    {
        public int ProductId { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    // Update product attributes
    public class UpdateProductAttributesCommand : IRequest<Dictionary<string, string>>
    {
        public int ProductId { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    // Create maintenance request
    public class CreateMaintenanceRequestCommand : IRequest<MaintenanceRequestDto>
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
    }

    // Update maintenance request
    public class UpdateMaintenanceRequestCommand : IRequest<MaintenanceRequestDto>
    {
        public int MaintenanceRequestId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    // Complete maintenance request
    public class CompleteMaintenanceRequestCommand : IRequest<MaintenanceRequestDto>
    {
        public int MaintenanceRequestId { get; set; }
    }

    // Bulk update product status
    public class BulkUpdateProductStatusCommand : IRequest<Dictionary<string, object>>
    {
        public List<int> ProductIds { get; set; } = new();
        public string Status { get; set; }
        public string Reason { get; set; }
        public MaintenanceScheduleDto MaintenanceSchedule { get; set; }
    }

    // Remove photo from product
    public class RemoveProductPhotoCommand : IRequest<bool>
    {
        public int PhotoId { get; set; }
    }

    // Set primary photo
    public class SetPrimaryPhotoCommand : IRequest<ProductPhotoDetailDto>
    {
        public int ProductId { get; set; }
        public int PhotoId { get; set; }
    }

    // Remove amenity from unit
    public class RemoveAmenityFromUnitCommand : IRequest<bool>
    {
        public int UnitProductId { get; set; }
        public int AmenityProductId { get; set; }
    }

    // Reorder photos
    public class ReorderProductPhotosCommand : IRequest<List<ProductPhotoDetailDto>>
    {
        public int ProductId { get; set; }
        public Dictionary<int, int> PhotoIdToSortOrder { get; set; } = new();
    }
}
