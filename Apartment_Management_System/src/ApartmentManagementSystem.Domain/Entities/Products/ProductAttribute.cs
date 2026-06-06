using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Products
{
    public class ProductAttribute : AuditableEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
