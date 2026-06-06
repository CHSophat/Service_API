using System;

namespace ApartmentManagementSystem.Application.DTOs.MessagingDto
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public int? PropertyId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
