using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class ChecklistItem : AuditableEntity
    {
        public int Id { get; set; }
        public int ChecklistId { get; set; }
        public string ItemName { get; set; }
        public string Condition { get; set; } // excellent, good, fair, poor
        public string Notes { get; set; }
        public bool IsDamage { get; set; } = false;
        public List<string> PhotoUrls { get; set; } = new List<string>();

        // Navigation properties
        public virtual MoveChecklist Checklist { get; set; }
    }
}
