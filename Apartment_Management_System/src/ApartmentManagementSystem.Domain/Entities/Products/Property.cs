using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Products;

public class Property : AuditableEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "KH";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int TotalUnits { get; set; }
    public string Status { get; set; } = "active";

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<PropertyOwner> PropertyOwners { get; set; } = new List<PropertyOwner>();
    public virtual ICollection<PropertyPhoto> PropertyPhotos { get; set; } = new List<PropertyPhoto>();
}
