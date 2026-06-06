using System;

namespace ApartmentManagementSystem.Application.DTOs.MessagingDto
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string Kind { get; set; } = "text";
        public string? Body { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
