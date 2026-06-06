using System;

namespace ApartmentManagementSystem.Application.DTOs.PropertyDto
{
    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string Priority { get; set; } = "medium";
        public string Description { get; set; } = string.Empty;
        public string? PhotoUrls { get; set; }
        public string Status { get; set; } = "open";
        public int? AssignedTo { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
