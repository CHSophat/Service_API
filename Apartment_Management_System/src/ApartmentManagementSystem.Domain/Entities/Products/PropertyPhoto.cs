namespace ApartmentManagementSystem.Domain.Entities.Products;

public class PropertyPhoto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Property Property { get; set; } = null!;
}
