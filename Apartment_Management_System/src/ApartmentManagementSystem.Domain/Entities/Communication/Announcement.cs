using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Communication;

public class Announcement : AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "all";
    public int? PropertyId { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? PublishAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string Status { get; set; } = "draft";
    public int? CreatedBy { get; set; }

    public virtual ICollection<AnnouncementDelivery> AnnouncementDeliveries { get; set; } = new List<AnnouncementDelivery>();
}
