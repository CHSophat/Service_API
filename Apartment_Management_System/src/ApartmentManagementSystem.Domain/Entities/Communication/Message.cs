using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Domain.Entities.Communication;

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public string Kind { get; set; } = "text";  // text, image, file, system
    public string? Body { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User SenderUser { get; set; } = null!;
}
