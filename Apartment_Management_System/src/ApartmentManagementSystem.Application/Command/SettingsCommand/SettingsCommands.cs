using ApartmentManagementSystem.Application.DTOs.SettingsDto;
using ApartmentManagementSystem.Domain.Entities;
using ApartmentManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApartmentManagementSystem.Application.Command.SettingsCommand;

public class UpdateBrandingCommand : IRequest<BrandingDto>
{
    public string? OrgName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Locale { get; set; }
}

public class UpdateBrandingCommandHandler : IRequestHandler<UpdateBrandingCommand, BrandingDto>
{
    private readonly ApplicationDbContext _ctx;

    public UpdateBrandingCommandHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<BrandingDto> Handle(UpdateBrandingCommand req, CancellationToken ct)
    {
        var branding = await _ctx.Set<Branding>().FirstOrDefaultAsync(ct) 
            ?? new Branding { CreatedAt = DateTime.UtcNow };

        if (!string.IsNullOrWhiteSpace(req.OrgName))
            branding.OrgName = req.OrgName;

        if (!string.IsNullOrWhiteSpace(req.PrimaryColor))
            branding.PrimaryColor = req.PrimaryColor;

        if (!string.IsNullOrWhiteSpace(req.AccentColor))
            branding.AccentColor = req.AccentColor;

        if (!string.IsNullOrWhiteSpace(req.ContactEmail))
            branding.ContactEmail = req.ContactEmail;

        if (!string.IsNullOrWhiteSpace(req.ContactPhone))
            branding.ContactPhone = req.ContactPhone;

        if (!string.IsNullOrWhiteSpace(req.Locale))
            branding.Locale = req.Locale;

        branding.UpdatedAt = DateTime.UtcNow;

        if (branding.Id == 0)
            _ctx.Set<Branding>().Add(branding);
        else
            _ctx.Set<Branding>().Update(branding);

        await _ctx.SaveChangesAsync(ct);

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

public class UploadBrandingLogoCommand : IRequest<UploadLogoResponse>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class UploadBrandingLogoCommandHandler : IRequestHandler<UploadBrandingLogoCommand, UploadLogoResponse>
{
    private readonly ApplicationDbContext _ctx;
    private readonly string _uploadsBasePath;

    public UploadBrandingLogoCommandHandler(ApplicationDbContext ctx)
    {
        _ctx = ctx;
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<UploadLogoResponse> Handle(UploadBrandingLogoCommand req, CancellationToken ct)
    {
        if (req.FileData == null || req.FileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(req.ContentType))
            throw new ArgumentException("Invalid file type. Only PNG, JPEG, GIF, and WebP are allowed.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (req.FileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 5MB.");

        // Save file
        var uploadsFolder = Path.Combine(_uploadsBasePath, "branding");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"logo_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{Path.GetExtension(req.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await File.WriteAllBytesAsync(filePath, req.FileData, ct);

        var logoUrl = $"/uploads/branding/{fileName}";

        // Update branding
        var branding = await _ctx.Set<Branding>().FirstOrDefaultAsync(ct) 
            ?? new Branding { CreatedAt = DateTime.UtcNow };

        branding.LogoUrl = logoUrl;
        branding.UpdatedAt = DateTime.UtcNow;

        if (branding.Id == 0)
            _ctx.Set<Branding>().Add(branding);
        else
            _ctx.Set<Branding>().Update(branding);

        await _ctx.SaveChangesAsync(ct);

        return new UploadLogoResponse
        {
            LogoUrl = logoUrl,
            FileName = req.FileName,
            FileSize = req.FileData.Length,
        };
    }
}
