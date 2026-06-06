using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.CommunicationDto;
using ApartmentManagementSystem.Domain.Entities.Communication;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.AnnouncementQuery;

public class GetAnnouncementsQuery : IRequest<List<AnnouncementDto>>
{
    public string? Status { get; set; }
    public string? Audience { get; set; }
    public int? PropertyId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, List<AnnouncementDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetAnnouncementsQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<AnnouncementDto>> Handle(GetAnnouncementsQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

        var q = _ctx.Announcements.AsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(a => a.Status == req.Status);
        
        if (!string.IsNullOrWhiteSpace(req.Audience))
            q = q.Where(a => a.Audience == req.Audience);
        
        if (req.PropertyId.HasValue)
            q = q.Where(a => a.PropertyId == req.PropertyId.Value);

        var rows = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(MapToDto).ToList();
    }

    internal static AnnouncementDto MapToDto(Announcement e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Body = e.Body,
        Audience = e.Audience,
        PropertyId = e.PropertyId,
        CoverUrl = e.CoverUrl,
        PublishAt = e.PublishAt,
        SentAt = e.SentAt,
        Status = e.Status,
        CreatedBy = e.CreatedBy,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}

public class GetAnnouncementByIdQuery : IRequest<AnnouncementDto?>
{
    public int Id { get; set; }
}

public class GetAnnouncementByIdQueryHandler : IRequestHandler<GetAnnouncementByIdQuery, AnnouncementDto?>
{
    private readonly ApplicationDbContext _ctx;
    public GetAnnouncementByIdQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<AnnouncementDto?> Handle(GetAnnouncementByIdQuery req, CancellationToken ct)
    {
        var e = await _ctx.Announcements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        return e is null ? null : GetAnnouncementsQueryHandler.MapToDto(e);
    }
}

public class GetAnnouncementDeliveriesQuery : IRequest<List<AnnouncementDeliveryDto>>
{
    public int AnnouncementId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetAnnouncementDeliveriesQueryHandler : IRequestHandler<GetAnnouncementDeliveriesQuery, List<AnnouncementDeliveryDto>>
{
    private readonly ApplicationDbContext _ctx;

    public GetAnnouncementDeliveriesQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<AnnouncementDeliveryDto>> Handle(GetAnnouncementDeliveriesQuery req, CancellationToken ct)
    {
        var deliveries = await _ctx.AnnouncementDeliveries
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.AnnouncementId == req.AnnouncementId)
            .OrderByDescending(d => d.DeliveredAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        return deliveries.Select(d => new AnnouncementDeliveryDto
        {
            Id = d.Id,
            AnnouncementId = d.AnnouncementId,
            UserId = d.UserId,
            UserName = d.User?.Email ?? string.Empty,
            UserEmail = d.User?.Email ?? string.Empty,
            DeliveredAt = d.DeliveredAt,
            OpenedAt = d.OpenedAt,
            IsOpened = d.OpenedAt.HasValue,
            MinutesToOpen = d.OpenedAt.HasValue ? (int?)((d.OpenedAt.Value - d.DeliveredAt).TotalMinutes) : null,
        }).ToList();
    }
}
