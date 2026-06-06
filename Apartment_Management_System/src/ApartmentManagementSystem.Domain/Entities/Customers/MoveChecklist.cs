using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class MoveChecklist : AuditableEntity
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string ChecklistType { get; set; } // move_in, move_out
        public DateTime InspectionDate { get; set; }
        public string InspectorName { get; set; }
        public string OverallCondition { get; set; } // excellent, good, fair, poor
        public string Notes { get; set; }

        // Navigation properties
        public virtual Lease Lease { get; set; }
        public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
    }
}
