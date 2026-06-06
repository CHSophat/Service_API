using ApartmentManagementSystem.Domain.Entities.Marketing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Marketing
{
    public class ListingPhotoConfiguration : IEntityTypeConfiguration<ListingPhoto>
    {
        public void Configure(EntityTypeBuilder<ListingPhoto> builder)
        {
            builder.ToTable("listing_photos", schema: "marketing");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PhotoUrl)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.Caption)
                .HasMaxLength(255);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Listing)
                .WithMany(x => x.ListingPhotos)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.ListingId);
        }
    }
}
