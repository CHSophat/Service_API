using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Products
{
    public class ProductPhoto : AuditableEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
