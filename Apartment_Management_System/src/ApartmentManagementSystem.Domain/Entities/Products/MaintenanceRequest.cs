using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Products
{
    public class MaintenanceRequest : AuditableEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string Priority { get; set; } = "medium"; // low, medium, high, emergency
        public string Description { get; set; } = string.Empty;
        public string? PhotoUrls { get; set; } // JSON-encoded array, nullable in DB
        public string Status { get; set; } = "open"; // open, assigned, in_progress, completed
        public int? AssignedTo { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
