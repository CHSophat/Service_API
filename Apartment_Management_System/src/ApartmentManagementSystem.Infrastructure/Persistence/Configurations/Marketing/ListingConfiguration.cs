using ApartmentManagementSystem.Domain.Entities.Marketing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Marketing
{
    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.ToTable("listings", schema: "marketing");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Slug)
                .HasMaxLength(220)
                .HasAnnotation("Unique", true);

            builder.Property(x => x.Headline)
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasColumnType("text");

            builder.Property(x => x.CoverPhotoUrl)
                .HasColumnType("text");

            builder.Property(x => x.MonthlyRent)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.Status)
                .HasDefaultValue("draft")
                .HasMaxLength(20);

            builder.Property(x => x.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.ListingPhotos)
                .WithOne(x => x.Listing)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsFeatured)
                .HasFilter("is_featured = TRUE");
            builder.HasIndex(x => x.PropertyId);
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.SortOrder);
        }
    }
}
