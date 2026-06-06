using ApartmentManagementSystem.Application.DTOs.SettingsDto;
using ApartmentManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagementSystem.Application.Query.SettingsQuery;

public class GetBrandingQuery : IRequest<BrandingDto>
{
}

public class GetBrandingQueryHandler : IRequestHandler<GetBrandingQuery, BrandingDto>
{
    private readonly ApplicationDbContext _ctx;

    public GetBrandingQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<BrandingDto> Handle(GetBrandingQuery req, CancellationToken ct)
    {
        var branding = await _ctx.Set<ApartmentManagementSystem.Domain.Entities.Branding>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (branding == null)
        {
            // Return default branding
            return new BrandingDto
            {
                Id = 0,
                OrgName = "Apartment Management System",
                PrimaryColor = "#1976d2",
                AccentColor = "#ff4081",
                Locale = "en-US",
            };
        }

        return new BrandingDto
        {
            Id = branding.Id,
            OrgName = branding.OrgName,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            AccentColor = branding.AccentColor,
            FaviconUrl = branding.FaviconUrl,
            ContactEmail = branding.ContactEmail,
            ContactPhone = branding.ContactPhone,
            Locale = branding.Locale,
            CreatedAt = branding.CreatedAt,
            UpdatedAt = branding.UpdatedAt,
        };
    }
}

public class GetAuditLogQuery : IRequest<List<AuditLogDto>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, List<AuditLogDto>>
{
    private readonly ApplicationDbContext _ctx;

    public GetAuditLogQueryHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 50 : req.PageSize;

        var query = _ctx.AuditLogs.AsNoTracking().AsQueryable();

        if (req.From.HasValue)
            query = query.Where(a => a.CreatedAt >= req.From);

        if (req.To.HasValue)
            query = query.Where(a => a.CreatedAt <= req.To);

        if (req.UserId.HasValue)
            query = query.Where(a => a.UserId == req.UserId);

        if (!string.IsNullOrWhiteSpace(req.Action))
            query = query.Where(a => a.Action.Contains(req.Action));

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return logs.Select(MapToDto).ToList();
    }

    private static AuditLogDto MapToDto(ApartmentManagementSystem.Domain.Entities.Auth.AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        Action = log.Action,
        EntityName = log.EntityName,
        EntityId = log.EntityId,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        Timestamp = log.Timestamp,
        CreatedAt = log.CreatedAt,
    };
}
