using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.PropertyDto;
using ApartmentManagementSystem.Domain.Entities.Products;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.PropertyCommand;

// =============================================================================
// Create
// =============================================================================
public class CreatePropertyCommand : IRequest<PropertyDto>
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

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreatePropertyCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<PropertyDto> Handle(CreatePropertyCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            throw new ArgumentException("code is required", nameof(req.Code));
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new ArgumentException("name is required", nameof(req.Name));

        var existing = await _ctx.Properties
            .AnyAsync(p => p.Code == req.Code, ct);
        if (existing)
            throw new InvalidOperationException($"Property code '{req.Code}' already exists.");

        var entity = new Property
        {
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            Description = req.Description,
            AddressLine = req.AddressLine,
            City = req.City,
            State = req.State,
            PostalCode = req.PostalCode,
            Country = string.IsNullOrWhiteSpace(req.Country) ? "KH" : req.Country!,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            TotalUnits = req.TotalUnits ?? 0,
            Status = "active",
        };
        _ctx.Properties.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return PropertyMapper.ToDto(entity);
    }
}

// =============================================================================
// Update / Delete (used by PUT /properties/{id} and DELETE /properties/{id})
// =============================================================================
public class UpdatePropertyCommand : IRequest<PropertyDto?>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public int? TotalUnits { get; set; }
    public string? Status { get; set; }
}

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto?>
{
    private readonly ApplicationDbContext _ctx;
    public UpdatePropertyCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<PropertyDto?> Handle(UpdatePropertyCommand req, CancellationToken ct)
    {
        var p = await _ctx.Properties.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (p is null) return null;

        if (req.Name is not null) p.Name = req.Name;
        if (req.Description is not null) p.Description = req.Description;
        if (req.AddressLine is not null) p.AddressLine = req.AddressLine;
        if (req.City is not null) p.City = req.City;
        if (req.State is not null) p.State = req.State;
        if (req.PostalCode is not null) p.PostalCode = req.PostalCode;
        if (req.Country is not null) p.Country = req.Country;
        if (req.TotalUnits.HasValue) p.TotalUnits = req.TotalUnits.Value;
        if (req.Status is not null) p.Status = req.Status;

        await _ctx.SaveChangesAsync(ct);
        return PropertyMapper.ToDto(p);
    }
}

public class DeletePropertyCommand : IRequest<bool>
{
    public int Id { get; set; }
}

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly ApplicationDbContext _ctx;
    public DeletePropertyCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<bool> Handle(DeletePropertyCommand req, CancellationToken ct)
    {
        var p = await _ctx.Properties.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (p is null) return false;
        // Soft-delete via status so existing Products/PropertyOwners FKs survive.
        p.Status = "archived";
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}

internal static class PropertyMapper
{
    public static PropertyDto ToDto(Property p) => new()
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
