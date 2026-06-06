using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.PropertyDto;
using ApartmentManagementSystem.Domain.Entities.Products;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.MaintenanceCommand;

public class CreateMaintenanceRequestCommand : IRequest<MaintenanceRequestDto>
{
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public required string Priority { get; set; }
    public required string Description { get; set; }
    public List<string>? PhotoUrls { get; set; }
}

public class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, MaintenanceRequestDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreateMaintenanceRequestCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<MaintenanceRequestDto> Handle(CreateMaintenanceRequestCommand req, CancellationToken ct)
    {
        if (req.ProductId <= 0)
            throw new ArgumentException("productId is required", nameof(req.ProductId));
        if (req.CustomerId <= 0)
            throw new ArgumentException("customerId is required", nameof(req.CustomerId));
        if (string.IsNullOrWhiteSpace(req.Description))
            throw new ArgumentException("description is required", nameof(req.Description));

        var validPriorities = new[] { "low", "medium", "high", "emergency" };
        if (!validPriorities.Contains(req.Priority?.ToLower()))
            throw new ArgumentException("priority must be one of: low, medium, high, emergency", nameof(req.Priority));

        var cleanedUrls = (req.PhotoUrls ?? new List<string>())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .ToList();

        var entity = new MaintenanceRequest
        {
            ProductId = req.ProductId,
            CustomerId = req.CustomerId,
            Priority = req.Priority.ToLower(),
            Description = req.Description.Trim(),
            PhotoUrls = JsonSerializer.Serialize(cleanedUrls),
            Status = "open",
        };
        _ctx.MaintenanceRequests.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    private static MaintenanceRequestDto ToDto(MaintenanceRequest e) => new()
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
