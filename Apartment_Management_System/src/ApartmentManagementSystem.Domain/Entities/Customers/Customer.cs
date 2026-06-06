using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Domain.Entities.Base;
using ApartmentManagementSystem.Domain.Entities.Financial;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class Customer : AuditableEntity
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string CustomerType { get; set; } = "tenant"; // tenant, owner, both
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // Optional contact/identity columns — DB schema permits NULL so the
        // CLR property must be nullable; otherwise EF's materialiser throws
        // InvalidCastException for any existing row with a NULL value.
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? GovernmentId { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // Navigation properties
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public virtual ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();
        public virtual ICollection<CustomerNote> Notes { get; set; } = new List<CustomerNote>();
        public virtual ICollection<PropertyOwner> PropertyOwners { get; set; } = new List<PropertyOwner>();
        public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}
