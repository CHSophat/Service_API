using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.MarketingDto;
using ApartmentManagementSystem.Domain.Entities.Marketing;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.ListingQuery;

public class GetListingsQuery : IRequest<List<ListingDto>>
{
    public string? Status { get; set; }
    public bool? Featured { get; set; }
    public string? Order { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetListingsQueryHandler : IRequestHandler<GetListingsQuery, List<ListingDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetListingsQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<ListingDto>> Handle(GetListingsQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

        var q = _ctx.Listings.AsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(l => l.Status == req.Status);
        
        if (req.Featured.HasValue)
            q = q.Where(l => l.IsFeatured == req.Featured.Value);

        var orderBy = req.Order?.ToLower() ?? "newest";
        q = orderBy switch
        {
            "oldest" => q.OrderBy(l => l.CreatedAt),
            "title" => q.OrderBy(l => l.Title),
            "featured" => q.OrderByDescending(l => l.IsFeatured).ThenByDescending(l => l.UpdatedAt),
            _ => q.OrderByDescending(l => l.UpdatedAt)
        };

        var rows = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(MapToDto).ToList();
    }

    internal static ListingDto MapToDto(Listing e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Slug = e.Slug,
        Headline = e.Headline,
        Description = e.Description,
        CoverPhotoUrl = e.CoverPhotoUrl,
        PropertyId = e.PropertyId,
        ProductId = e.ProductId,
        MonthlyRent = e.MonthlyRent,
        AvailableFrom = e.AvailableFrom,
        Status = e.Status,
        IsFeatured = e.IsFeatured,
        SortOrder = e.SortOrder,
        PublishedAt = e.PublishedAt,
        CreatedBy = e.CreatedBy,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}

public class GetListingByIdQuery : IRequest<ListingDto?>
{
    public int Id { get; set; }
}

public class GetListingByIdQueryHandler : IRequestHandler<GetListingByIdQuery, ListingDto?>
{
    private readonly ApplicationDbContext _ctx;
    public GetListingByIdQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto?> Handle(GetListingByIdQuery req, CancellationToken ct)
    {
        var e = await _ctx.Listings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        return e is null ? null : GetListingsQueryHandler.MapToDto(e);
    }
}
