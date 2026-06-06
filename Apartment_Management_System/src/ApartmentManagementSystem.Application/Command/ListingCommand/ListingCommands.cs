using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.MarketingDto;
using ApartmentManagementSystem.Domain.Entities.Marketing;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.ListingCommand;

public class CreateListingCommand : IRequest<ListingDto>
{
    public required string Title { get; set; }
    public string? Slug { get; set; }
    public string? Headline { get; set; }
    public string? Description { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public int? PropertyId { get; set; }
    public int? ProductId { get; set; }
    public decimal? MonthlyRent { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public int? CreatedBy { get; set; }
}

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, ListingDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreateListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto> Handle(CreateListingCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ArgumentException("title is required", nameof(req.Title));

        var entity = new Listing
        {
            Title = req.Title.Trim(),
            Slug = req.Slug,
            Headline = req.Headline,
            Description = req.Description,
            CoverPhotoUrl = req.CoverPhotoUrl,
            PropertyId = req.PropertyId,
            ProductId = req.ProductId,
            MonthlyRent = req.MonthlyRent,
            AvailableFrom = req.AvailableFrom,
            Status = "draft",
            IsFeatured = req.IsFeatured,
            SortOrder = req.SortOrder,
            CreatedBy = req.CreatedBy,
        };
        _ctx.Listings.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    internal static ListingDto ToDto(Listing e) => new()
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

// =============================================================================
// Update
// =============================================================================
public class UpdateListingCommand : IRequest<ListingDto?>
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Headline { get; set; }
    public string? Description { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public int? PropertyId { get; set; }
    public int? ProductId { get; set; }
    public decimal? MonthlyRent { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public int? SortOrder { get; set; }
}

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand, ListingDto?>
{
    private readonly ApplicationDbContext _ctx;
    public UpdateListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto?> Handle(UpdateListingCommand req, CancellationToken ct)
    {
        var e = await _ctx.Listings.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        if (!string.IsNullOrWhiteSpace(req.Title)) e.Title = req.Title.Trim();
        if (req.Slug is not null) e.Slug = req.Slug;
        if (req.Headline is not null) e.Headline = req.Headline;
        if (req.Description is not null) e.Description = req.Description;
        if (req.CoverPhotoUrl is not null) e.CoverPhotoUrl = req.CoverPhotoUrl;
        if (req.PropertyId.HasValue) e.PropertyId = req.PropertyId;
        if (req.ProductId.HasValue) e.ProductId = req.ProductId;
        if (req.MonthlyRent.HasValue) e.MonthlyRent = req.MonthlyRent;
        if (req.AvailableFrom.HasValue) e.AvailableFrom = req.AvailableFrom;
        if (req.SortOrder.HasValue) e.SortOrder = req.SortOrder.Value;

        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateListingCommandHandler.ToDto(e);
    }
}

// =============================================================================
// Delete (archive)
// =============================================================================
public class DeleteListingCommand : IRequest<bool>
{
    public int Id { get; set; }
}

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand, bool>
{
    private readonly ApplicationDbContext _ctx;
    public DeleteListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<bool> Handle(DeleteListingCommand req, CancellationToken ct)
    {
        var e = await _ctx.Listings.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return false;

        e.Status = "archived";
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}

// =============================================================================
// Publish / Unpublish
// =============================================================================
public class PublishListingCommand : IRequest<ListingDto?>
{
    public int Id { get; set; }
}

public class PublishListingCommandHandler : IRequestHandler<PublishListingCommand, ListingDto?>
{
    private readonly ApplicationDbContext _ctx;
    public PublishListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto?> Handle(PublishListingCommand req, CancellationToken ct)
    {
        var e = await _ctx.Listings.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        e.Status = "published";
        e.PublishedAt = DateTime.UtcNow;
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateListingCommandHandler.ToDto(e);
    }
}

public class UnpublishListingCommand : IRequest<ListingDto?>
{
    public int Id { get; set; }
}

public class UnpublishListingCommandHandler : IRequestHandler<UnpublishListingCommand, ListingDto?>
{
    private readonly ApplicationDbContext _ctx;
    public UnpublishListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto?> Handle(UnpublishListingCommand req, CancellationToken ct)
    {
        var e = await _ctx.Listings.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        e.Status = "unpublished";
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateListingCommandHandler.ToDto(e);
    }
}

// =============================================================================
// Feature toggle
// =============================================================================
public class SetFeaturedListingCommand : IRequest<ListingDto?>
{
    public int Id { get; set; }
    public bool IsFeatured { get; set; } = true;
}

public class SetFeaturedListingCommandHandler : IRequestHandler<SetFeaturedListingCommand, ListingDto?>
{
    private readonly ApplicationDbContext _ctx;
    public SetFeaturedListingCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<ListingDto?> Handle(SetFeaturedListingCommand req, CancellationToken ct)
    {
        var e = await _ctx.Listings.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (e is null) return null;

        e.IsFeatured = req.IsFeatured;
        e.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
        return CreateListingCommandHandler.ToDto(e);
    }
}
