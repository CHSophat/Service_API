using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class Address : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string AddressType { get; set; } // billing, mailing, property
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsPrimary { get; set; } = false;

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
}
