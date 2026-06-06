namespace ApartmentManagementSystem.Application.DTOs.AnnouncementDto;

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "all"; // all, tenants, owners, managers, staff
    public int? PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? PublishAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string Status { get; set; } = "draft"; // draft, scheduled, sent
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DeliveryCount { get; set; }
    public int OpenCount { get; set; }
    public decimal OpenRate { get; set; } // percentage
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "all"; // all, tenants, owners, managers, staff
    public int? PropertyId { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? PublishAt { get; set; }
}

public class UpdateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "all";
    public int? PropertyId { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? PublishAt { get; set; }
}

public class AnnouncementDeliveryDto
{
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public bool IsOpened { get; set; }
    public int? MinutesToOpen { get; set; } // null if not opened
}

public class SendAnnouncementResponse
{
    public int AnnouncementId { get; set; }
    public int RecipientCount { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UploadCoverResponse
{
    public string CoverUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
