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

namespace ApartmentManagementSystem.Application.Query.MaintenanceQuery;

public class GetMaintenanceRequestsQuery : IRequest<List<MaintenanceRequestDto>>
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetMaintenanceRequestsQueryHandler : IRequestHandler<GetMaintenanceRequestsQuery, List<MaintenanceRequestDto>>
{
    private readonly ApplicationDbContext _ctx;
    public GetMaintenanceRequestsQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<List<MaintenanceRequestDto>> Handle(GetMaintenanceRequestsQuery req, CancellationToken ct)
    {
        var page = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

        var q = _ctx.MaintenanceRequests.AsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(m => m.Status == req.Status);
        
        if (!string.IsNullOrWhiteSpace(req.Priority))
            q = q.Where(m => m.Priority == req.Priority);
        
        if (req.ProductId.HasValue)
            q = q.Where(m => m.ProductId == req.ProductId.Value);

        var rows = await q
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return rows.Select(MapToDto).ToList();
    }

    internal static MaintenanceRequestDto MapToDto(MaintenanceRequest e) => new()
    {
        Id = e.Id,
        ProductId = e.ProductId,
        CustomerId = e.CustomerId,
        Priority = e.Priority,
        Description = e.Description,
        PhotoUrls = e.PhotoUrls,
        Status = e.Status,
        AssignedTo = e.AssignedTo,
        CompletedAt = e.CompletedAt,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}

public class GetMaintenanceRequestByIdQuery : IRequest<MaintenanceRequestDto?>
{
    public int Id { get; set; }
}

public class GetMaintenanceRequestByIdQueryHandler : IRequestHandler<GetMaintenanceRequestByIdQuery, MaintenanceRequestDto?>
{
    private readonly ApplicationDbContext _ctx;
    public GetMaintenanceRequestByIdQueryHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<MaintenanceRequestDto?> Handle(GetMaintenanceRequestByIdQuery req, CancellationToken ct)
    {
        var e = await _ctx.MaintenanceRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        return e is null ? null : GetMaintenanceRequestsQueryHandler.MapToDto(e);
    }
}
