using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class LeaseDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string DocumentType { get; set; } // lease_agreement, addendum, inspection_report
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; } // pending_review, approved, rejected
        public string Description { get; set; }
        public DateTime? SignedDate { get; set; }
        public string SignedBy { get; set; }

        // Navigation properties
        public virtual Lease Lease { get; set; }
    }
}
