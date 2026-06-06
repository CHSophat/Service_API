using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Marketing;

public class Listing : AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Headline { get; set; }
    public string? Description { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public int? PropertyId { get; set; }
    public int? ProductId { get; set; }
    public decimal? MonthlyRent { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public string Status { get; set; } = "draft";
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int? CreatedBy { get; set; }

    public virtual ICollection<ListingPhoto> ListingPhotos { get; set; } = new List<ListingPhoto>();
}
