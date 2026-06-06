using System;

namespace ApartmentManagementSystem.Application.DTOs.PropertyDto;

/// <summary>
/// Wire-format for product.properties rows. Mirrors AMS_MSI's
/// PropertyApiService.Property interface so the client can bind directly.
/// </summary>
public class PropertyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "KH";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int TotalUnits { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Aggregated KPIs, populated by the list query so clients don't have to
    // make a separate /kpis call per property (avoids an N+1 request storm).
    public double OccupancyPct { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OpenTickets { get; set; }
}

public class CreatePropertyRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? TotalUnits { get; set; }
}
