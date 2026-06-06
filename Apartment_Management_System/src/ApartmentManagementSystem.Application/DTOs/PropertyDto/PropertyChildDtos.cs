using System;

namespace ApartmentManagementSystem.Application.DTOs.PropertyDto;

public class PropertyOwnerDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int CustomerId { get; set; }
    public decimal OwnershipPct { get; set; }
    public bool IsPrimary { get; set; }
    public DateOnly? SinceDate { get; set; }
}

public class PropertyPhotoDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PropertyUnitDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? ProductType { get; set; }
    public string? Status { get; set; }
    public decimal BasePrice { get; set; }
    public int? PropertyId { get; set; }
    public int? FloorNumber { get; set; }
    public short? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? SquareFeet { get; set; }
}
