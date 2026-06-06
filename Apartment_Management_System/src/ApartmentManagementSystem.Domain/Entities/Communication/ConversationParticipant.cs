using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Domain.Entities.Communication;

public class ConversationParticipant
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
