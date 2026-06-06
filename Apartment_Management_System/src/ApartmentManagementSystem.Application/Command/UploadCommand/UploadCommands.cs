using ApartmentManagementSystem.Application.DTOs.UploadDto;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApartmentManagementSystem.Application.Command.UploadCommand;

public class UploadProfilePhotoCommand : IRequest<ProfilePhotoUploadDto>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
}

public class UploadProfilePhotoCommandHandler : IRequestHandler<UploadProfilePhotoCommand, ProfilePhotoUploadDto>
{
    private readonly string _uploadsBasePath;

    public UploadProfilePhotoCommandHandler()
    {
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<ProfilePhotoUploadDto> Handle(UploadProfilePhotoCommand req, CancellationToken ct)
    {
        return await UploadFileAsync(req.FileData, req.FileName, req.ContentType, "profile-photos", 
            new ProfilePhotoUploadDto { CustomerId = req.CustomerId }, ct);
    }

    private async Task<T> UploadFileAsync<T>(byte[] fileData, string fileName, string contentType, 
        string folderName, T dto, CancellationToken ct) where T : UploadResultDto
    {
        if (fileData == null || fileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(contentType))
            throw new ArgumentException("Invalid file type. Only image files are allowed.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (fileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 5MB.");

        var uploadsFolder = Path.Combine(_uploadsBasePath, folderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(fileName);
        var newFileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        await File.WriteAllBytesAsync(filePath, fileData, ct);

        dto.Url = $"/uploads/{folderName}/{newFileName}";
        dto.FileName = fileName;
        dto.ContentType = contentType;
        dto.FileSize = fileData.Length;
        dto.UploadedAt = DateTime.UtcNow;

        return dto;
    }
}

public class UploadMaintenancePhotoCommand : IRequest<MaintenancePhotoUploadDto>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int? MaintenanceRequestId { get; set; }
}

public class UploadMaintenancePhotoCommandHandler : IRequestHandler<UploadMaintenancePhotoCommand, MaintenancePhotoUploadDto>
{
    private readonly string _uploadsBasePath;

    public UploadMaintenancePhotoCommandHandler()
    {
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<MaintenancePhotoUploadDto> Handle(UploadMaintenancePhotoCommand req, CancellationToken ct)
    {
        return await UploadFileAsync(req.FileData, req.FileName, req.ContentType, "maintenance-photos",
            new MaintenancePhotoUploadDto { MaintenanceRequestId = req.MaintenanceRequestId }, ct);
    }

    private async Task<T> UploadFileAsync<T>(byte[] fileData, string fileName, string contentType, 
        string folderName, T dto, CancellationToken ct) where T : UploadResultDto
    {
        if (fileData == null || fileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(contentType))
            throw new ArgumentException("Invalid file type. Only image files are allowed.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (fileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 5MB.");

        var uploadsFolder = Path.Combine(_uploadsBasePath, folderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(fileName);
        var newFileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        await File.WriteAllBytesAsync(filePath, fileData, ct);

        dto.Url = $"/uploads/{folderName}/{newFileName}";
        dto.FileName = fileName;
        dto.ContentType = contentType;
        dto.FileSize = fileData.Length;
        dto.UploadedAt = DateTime.UtcNow;

        return dto;
    }
}

public class UploadPropertyPhotoCommand : IRequest<PropertyPhotoUploadDto>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int? PropertyId { get; set; }
}

public class UploadPropertyPhotoCommandHandler : IRequestHandler<UploadPropertyPhotoCommand, PropertyPhotoUploadDto>
{
    private readonly string _uploadsBasePath;

    public UploadPropertyPhotoCommandHandler()
    {
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<PropertyPhotoUploadDto> Handle(UploadPropertyPhotoCommand req, CancellationToken ct)
    {
        return await UploadFileAsync(req.FileData, req.FileName, req.ContentType, "property-photos",
            new PropertyPhotoUploadDto { PropertyId = req.PropertyId }, ct);
    }

    private async Task<T> UploadFileAsync<T>(byte[] fileData, string fileName, string contentType, 
        string folderName, T dto, CancellationToken ct) where T : UploadResultDto
    {
        if (fileData == null || fileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(contentType))
            throw new ArgumentException("Invalid file type. Only image files are allowed.");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (fileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 5MB.");

        var uploadsFolder = Path.Combine(_uploadsBasePath, folderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(fileName);
        var newFileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        await File.WriteAllBytesAsync(filePath, fileData, ct);

        dto.Url = $"/uploads/{folderName}/{newFileName}";
        dto.FileName = fileName;
        dto.ContentType = contentType;
        dto.FileSize = fileData.Length;
        dto.UploadedAt = DateTime.UtcNow;

        return dto;
    }
}

public class UploadDocumentCommand : IRequest<DocumentUploadDto>
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentUploadDto>
{
    private readonly string _uploadsBasePath;

    public UploadDocumentCommandHandler()
    {
        _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<DocumentUploadDto> Handle(UploadDocumentCommand req, CancellationToken ct)
    {
        if (req.FileData == null || req.FileData.Length == 0)
            throw new ArgumentException("No file provided");

        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar" };
        var fileExtension = Path.GetExtension(req.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

        const long maxSize = 25 * 1024 * 1024; // 25MB
        if (req.FileData.Length > maxSize)
            throw new ArgumentException("File is too large. Maximum size is 25MB.");

        var uploadsFolder = Path.Combine(_uploadsBasePath, "documents");
        Directory.CreateDirectory(uploadsFolder);

        var newFileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        await File.WriteAllBytesAsync(filePath, req.FileData, ct);

        var dto = new DocumentUploadDto
        {
            Url = $"/uploads/documents/{newFileName}",
            FileName = req.FileName,
            ContentType = req.ContentType,
            FileSize = req.FileData.Length,
            UploadedAt = DateTime.UtcNow,
            Category = req.Category,
            DocumentType = fileExtension.TrimStart('.').ToUpper(),
        };

        return dto;
    }
}
