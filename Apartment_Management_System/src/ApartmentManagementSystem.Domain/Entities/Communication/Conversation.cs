using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Communication;

public class Conversation : AuditableEntity
{
    public int Id { get; set; }
    public string? Subject { get; set; }
    public int? PropertyId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
