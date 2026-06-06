using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.MarketingDto
{
    public class ListingDto
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
