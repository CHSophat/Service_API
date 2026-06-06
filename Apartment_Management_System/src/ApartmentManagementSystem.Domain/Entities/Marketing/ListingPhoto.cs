namespace ApartmentManagementSystem.Domain.Entities.Marketing;

public class ListingPhoto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Listing Listing { get; set; } = null!;
}
