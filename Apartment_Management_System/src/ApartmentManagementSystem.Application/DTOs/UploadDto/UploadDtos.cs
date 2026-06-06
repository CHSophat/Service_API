namespace ApartmentManagementSystem.Application.DTOs.UploadDto;

public class UploadResultDto
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ProfilePhotoUploadDto : UploadResultDto
{
    public int? CustomerId { get; set; }
}

public class MaintenancePhotoUploadDto : UploadResultDto
{
    public int? MaintenanceRequestId { get; set; }
}

public class PropertyPhotoUploadDto : UploadResultDto
{
    public int? PropertyId { get; set; }
}

public class DocumentUploadDto : UploadResultDto
{
    public string? Category { get; set; }
    public string? DocumentType { get; set; }
}
