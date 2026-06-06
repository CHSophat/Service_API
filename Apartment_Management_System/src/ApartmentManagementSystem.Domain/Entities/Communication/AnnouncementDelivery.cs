using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Domain.Entities.Communication;

public class AnnouncementDelivery
{
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public int UserId { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
    public DateTime? OpenedAt { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
