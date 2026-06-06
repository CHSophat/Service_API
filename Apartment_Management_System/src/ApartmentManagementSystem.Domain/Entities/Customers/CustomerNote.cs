using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class CustomerNote : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
}
