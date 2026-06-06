using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.CommunicationDto;
using ApartmentManagementSystem.Domain.Entities.Communication;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.AnnouncementCommand;

public class CreateAnnouncementCommand : IRequest<AnnouncementDto>
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string? Audience { get; set; } = "all";
    public int? PropertyId { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? PublishAt { get; set; }
    public int? CreatedBy { get; set; }
}

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, AnnouncementDto>
{
    private readonly ApplicationDbContext _ctx;
    public CreateAnnouncementCommandHandler(ApplicationDbContext ctx) { _ctx = ctx; }

    public async Task<AnnouncementDto> Handle(CreateAnnouncementCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ArgumentException("title is required", nameof(req.Title));
        if (string.IsNullOrWhiteSpace(req.Body))
            throw new ArgumentException("body is required", nameof(req.Body));

        if (req.PropertyId is int pid)
        {
            if (pid <= 0)
                throw new ArgumentException("propertyId must be a positive integer", nameof(req.PropertyId));
            var propertyExists = await _ctx.Properties.AnyAsync(p => p.Id == pid, ct);
            if (!propertyExists)
                throw new ArgumentException($"Property with id {pid} does not exist.", nameof(req.PropertyId));
        }

        var entity = new Announcement
        {
            Title = req.Title.Trim(),
            Body = req.Body.Trim(),
            Audience = req.Audience ?? "all",
            PropertyId = req.PropertyId,
            CoverUrl = req.CoverUrl,
            PublishAt = req.PublishAt,
            Status = "draft",
            CreatedBy = req.CreatedBy,
        };
        _ctx.Announcements.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    private static AnnouncementDto ToDto(Announcement e) => new()
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

public class SendAnnouncementNowCommand : IRequest<SendAnnouncementResponse>
{
    public int AnnouncementId { get; set; }
}

public class SendAnnouncementNowCommandHandler : IRequestHandler<SendAnnouncementNowCommand, SendAnnouncementResponse>
{
    private readonly ApplicationDbContext _ctx;

    public SendAnnouncementNowCommandHandler(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<SendAnnouncementResponse> Handle(SendAnnouncementNowCommand req, CancellationToken ct)
    {
        var announcement = await _ctx.Announcements
            .FirstOrDefaultAsync(a => a.Id == req.AnnouncementId, ct);

        if (announcement == null)
            throw new KeyNotFoundException($"Announcement {req.AnnouncementId} not found");

        // Get recipient list based on audience
        var recipients = await GetRecipients(announcement, ct);

        if (recipients.Count == 0)
            throw new InvalidOperationException("No recipients found for this announcement");

        // Create delivery records
        var now = DateTime.UtcNow;
        var deliveries = recipients.Select(userId => new AnnouncementDelivery
        {
            AnnouncementId = req.AnnouncementId,
            UserId = userId,
            DeliveredAt = now,
        }).ToList();

        _ctx.AnnouncementDeliveries.AddRange(deliveries);

        // Update announcement status
        announcement.Status = "sent";
        announcement.SentAt = now;
        announcement.UpdatedAt = now;

        await _ctx.SaveChangesAsync(ct);

        return new SendAnnouncementResponse
        {
            AnnouncementId = req.AnnouncementId,
            RecipientCount = deliveries.Count,
            SentAt = now,
            Message = $"Announcement sent to {deliveries.Count} recipients",
        };
    }

    private async Task<List<int>> GetRecipients(Announcement announcement, CancellationToken ct)
    {
        var query = _ctx.Users
            .Include(u => u.UserRoles)
            .Where(u => u.IsActive)
            .AsQueryable();

        return announcement.Audience switch
        {
            "tenants" => await query.Where(u => u.UserRoles.Any(r => r.Role!.Name == "Tenant")).Select(u => u.Id).ToListAsync(ct),
            "owners" => await query.Where(u => u.UserRoles.Any(r => r.Role!.Name == "Owner")).Select(u => u.Id).ToListAsync(ct),
            "managers" => await query.Where(u => u.UserRoles.Any(r => r.Role!.Name == "PropertyManager")).Select(u => u.Id).ToListAsync(ct),
            "staff" => await query.Where(u => u.UserRoles.Any(r => r.Role!.Name == "Staff")).Select(u => u.Id).ToListAsync(ct),
            _ => await query.Select(u => u.Id).ToListAsync(ct),
        };
    }
}

public class UploadAnnouncementCoverCommand : IRequest<UploadCoverResponse>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class UploadAnnouncementCoverCommandHandler : IRequestHandler<UploadAnnouncementCoverCommand, UploadCoverResponse>
{
    private readonly string _uploadsBasePath;

    public UploadAnnouncementCoverCommandHandler()
    {
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<UploadCoverResponse> Handle(UploadAnnouncementCoverCommand req, CancellationToken ct)
    {
        if (req.FileData == null || req.FileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(req.ContentType))
            throw new ArgumentException("Invalid file type. Only PNG, JPEG, GIF, and WebP are allowed.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (req.FileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 5MB.");

        var uploadsFolder = Path.Combine(_uploadsBasePath, "announcements");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"cover_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{Path.GetExtension(req.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await File.WriteAllBytesAsync(filePath, req.FileData, ct);

        var coverUrl = $"/uploads/announcements/{fileName}";

        return new UploadCoverResponse
        {
            CoverUrl = coverUrl,
            FileName = req.FileName,
            FileSize = req.FileData.Length,
        };
    }
}
