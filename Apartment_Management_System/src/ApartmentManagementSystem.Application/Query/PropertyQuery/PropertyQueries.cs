using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.PropertyDto;
using ApartmentManagementSystem.Domain.Entities.Products;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.PropertyQuery;

// =============================================================================
// List
// =============================================================================
public class GetPropertiesQuery : IRequest<List<PropertyDto>>
{
    public string? Status { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, List<PropertyDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertiesQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<PropertyDto>> Handle(GetPropertiesQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 50 : req.PageSize;

        var q = _ctx.Properties.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(p => p.Status == req.Status);
        if (!string.IsNullOrWhiteSpace(req.Q))
        {
            var term = req.Q.Trim().ToLower();
            q = q.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Code.ToLower().Contains(term));
        }

        var rows = await q
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = rows.Select(MapToDto).ToList();
        if (dtos.Count == 0) return dtos;

        // --- KPI enrichment for the whole page in 3 grouped round-trips ---
        // (previously the client issued one /kpis call per property → N+1).
        var ids = dtos.Select(d => (int?)d.Id).ToList();

        var unitStats = await _ctx.Products
            .Where(p => ids.Contains(p.PropertyId))
            .GroupBy(p => p.PropertyId)
            .Select(g => new
            {
                PropertyId = g.Key,
                Total = g.Count(),
                Occupied = g.Count(x => x.Status == "occupied"),
            })
            .ToListAsync(ct);

        var revenue = await _ctx.Leases
            .Where(l => l.Status == "active")
            .Join(_ctx.Products, l => l.ProductId, p => p.Id, (l, p) => new { p.PropertyId, l.MonthlyRent })
            .Where(x => ids.Contains(x.PropertyId))
            .GroupBy(x => x.PropertyId)
            .Select(g => new { PropertyId = g.Key, Revenue = g.Sum(x => x.MonthlyRent) })
            .ToListAsync(ct);

        var tickets = await _ctx.MaintenanceRequests
            .Where(m => m.Status != "completed")
            .Join(_ctx.Products, m => m.ProductId, p => p.Id, (m, p) => new { p.PropertyId })
            .Where(x => ids.Contains(x.PropertyId))
            .GroupBy(x => x.PropertyId)
            .Select(g => new { PropertyId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var unitMap = unitStats.ToDictionary(x => x.PropertyId);
        var revMap = revenue.ToDictionary(x => x.PropertyId, x => x.Revenue);
        var tickMap = tickets.ToDictionary(x => x.PropertyId, x => x.Count);

        foreach (var d in dtos)
        {
            if (unitMap.TryGetValue(d.Id, out var u))
            {
                d.OccupiedUnits = u.Occupied;
                d.OccupancyPct = u.Total > 0 ? Math.Round((double)u.Occupied / u.Total * 100, 1) : 0;
            }
            d.MonthlyRevenue = revMap.TryGetValue(d.Id, out var r) ? r : 0m;
            d.OpenTickets = tickMap.TryGetValue(d.Id, out var t) ? t : 0;
        }

        return dtos;
    }

    internal static PropertyDto MapToDto(Property p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Description = p.Description,
        AddressLine = p.AddressLine,
        City = p.City,
        State = p.State,
        PostalCode = p.PostalCode,
        Country = p.Country,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        TotalUnits = p.TotalUnits,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };
}

// =============================================================================
// By Id
// =============================================================================
public class GetPropertyByIdQuery : IRequest<PropertyDto?>
{
    public int Id { get; set; }
}

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertyByIdQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery req, CancellationToken ct)
    {
        var p = await _ctx.Properties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        return p is null ? null : GetPropertiesQueryHandler.MapToDto(p);
    }
}

// =============================================================================
// KPIs (occupancy / monthly revenue / open tickets)
// =============================================================================
public class GetPropertyKpisQuery : IRequest<PropertyKpisDto>
{
    public int Id { get; set; }
}

public class PropertyKpisDto
{
    public double OccupancyPct { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OpenTickets { get; set; }
}

public class GetPropertyKpisQueryHandler : IRequestHandler<GetPropertyKpisQuery, PropertyKpisDto>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertyKpisQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<PropertyKpisDto> Handle(GetPropertyKpisQuery req, CancellationToken ct)
    {
        // Occupancy: products in property whose status == 'occupied' / total units.
        var totalUnits = await _ctx.Products.CountAsync(p => p.PropertyId == req.Id, ct);
        var occupied = await _ctx.Products
            .CountAsync(p => p.PropertyId == req.Id && p.Status == "occupied", ct);

        // Monthly revenue: sum of monthly_rent on active leases attached to those units.
        var unitIds = await _ctx.Products
            .Where(p => p.PropertyId == req.Id)
            .Select(p => p.Id)
            .ToListAsync(ct);
        var revenue = await _ctx.Leases
            .Where(l => unitIds.Contains(l.ProductId) && l.Status == "active")
            .SumAsync(l => (decimal?)l.MonthlyRent, ct) ?? 0m;

        // Open tickets: maintenance requests on units in this property that aren't completed.
        var openTickets = await _ctx.MaintenanceRequests
            .Where(m => unitIds.Contains(m.ProductId) && m.Status != "completed")
            .CountAsync(ct);

        return new PropertyKpisDto
        {
            OccupancyPct = totalUnits > 0 ? Math.Round((double)occupied / totalUnits * 100, 1) : 0,
            MonthlyRevenue = revenue,
            OpenTickets = openTickets,
        };
    }
}

// =============================================================================
// Units (products attached to a property)
// =============================================================================
public class GetPropertyUnitsQuery : IRequest<List<PropertyUnitDto>>
{
    public int PropertyId { get; set; }
}

public class GetPropertyUnitsQueryHandler : IRequestHandler<GetPropertyUnitsQuery, List<PropertyUnitDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertyUnitsQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<PropertyUnitDto>> Handle(GetPropertyUnitsQuery req, CancellationToken ct)
    {
        return await _ctx.Products.AsNoTracking()
            .Where(p => p.PropertyId == req.PropertyId)
            .OrderBy(p => p.FloorNumber).ThenBy(p => p.Code)
            .Select(p => new PropertyUnitDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                ProductType = p.ProductType,
                Status = p.Status,
                BasePrice = p.BasePrice,
                PropertyId = p.PropertyId,
                FloorNumber = p.FloorNumber,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = p.SquareFeet,
            })
            .ToListAsync(ct);
    }
}

// =============================================================================
// Photos
// =============================================================================
public class GetPropertyPhotosQuery : IRequest<List<PropertyPhotoDto>>
{
    public int PropertyId { get; set; }
}

public class GetPropertyPhotosQueryHandler : IRequestHandler<GetPropertyPhotosQuery, List<PropertyPhotoDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertyPhotosQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<PropertyPhotoDto>> Handle(GetPropertyPhotosQuery req, CancellationToken ct)
    {
        return await _ctx.PropertyPhotos.AsNoTracking()
            .Where(ph => ph.PropertyId == req.PropertyId)
            .OrderByDescending(ph => ph.IsPrimary)
            .ThenBy(ph => ph.SortOrder)
            .ThenBy(ph => ph.Id)
            .Select(ph => new PropertyPhotoDto
            {
                Id = ph.Id,
                PropertyId = ph.PropertyId,
                PhotoUrl = ph.PhotoUrl,
                Caption = ph.Caption,
                IsPrimary = ph.IsPrimary,
                SortOrder = ph.SortOrder,
                CreatedAt = ph.CreatedAt,
            })
            .ToListAsync(ct);
    }
}

// =============================================================================
// Owners
// =============================================================================
public class GetPropertyOwnersQuery : IRequest<List<PropertyOwnerDto>>
{
    public int PropertyId { get; set; }
}

public class GetPropertyOwnersQueryHandler : IRequestHandler<GetPropertyOwnersQuery, List<PropertyOwnerDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetPropertyOwnersQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<PropertyOwnerDto>> Handle(GetPropertyOwnersQuery req, CancellationToken ct)
    {
        return await _ctx.PropertyOwners.AsNoTracking()
            .Where(o => o.PropertyId == req.PropertyId)
            .OrderByDescending(o => o.IsPrimary)
            .ThenBy(o => o.Id)
            .Select(o => new PropertyOwnerDto
            {
                Id = o.Id,
                PropertyId = o.PropertyId,
                CustomerId = o.CustomerId,
                OwnershipPct = o.OwnershipPct,
                IsPrimary = o.IsPrimary,
                SinceDate = o.SinceDate,
            })
            .ToListAsync(ct);
    }
}
