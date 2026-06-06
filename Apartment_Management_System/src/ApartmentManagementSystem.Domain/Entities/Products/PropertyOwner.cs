using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Domain.Entities.Products;

public class PropertyOwner
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int CustomerId { get; set; }
    public decimal OwnershipPct { get; set; } = 100m;
    public bool IsPrimary { get; set; }
    public DateOnly? SinceDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Property Property { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}
