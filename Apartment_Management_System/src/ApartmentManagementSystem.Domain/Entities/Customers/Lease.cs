using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class Lease : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public string Status { get; set; } = "active"; // active, expired, terminated
        public string SignedDocumentUrl { get; set; }
        public DateTime? SignedDate { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual ICollection<LeaseDocument> Documents { get; set; } = new List<LeaseDocument>();
        public virtual ICollection<MoveChecklist> Checklists { get; set; } = new List<MoveChecklist>();
    }
}
