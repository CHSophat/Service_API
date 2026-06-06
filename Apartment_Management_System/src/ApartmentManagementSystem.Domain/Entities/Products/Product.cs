using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Products
{
    public class Product : AuditableEntity
    {
        public int Id { get; set; }
        public string ProductType { get; set; } // unit, parking, storage, amenity
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "vacant"; // vacant, occupied, maintenance, unavailable
        public decimal BasePrice { get; set; }
        public int? SquareFeet { get; set; }
        public short? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public int? FloorNumber { get; set; }
        public int? PropertyId { get; set; }

        // Navigation properties
        public virtual Property? Property { get; set; }
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
        public virtual ICollection<ProductPhoto> ProductPhotos { get; set; } = new List<ProductPhoto>();
        public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
