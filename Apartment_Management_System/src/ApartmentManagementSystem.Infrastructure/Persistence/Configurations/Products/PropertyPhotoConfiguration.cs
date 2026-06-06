using ApartmentManagementSystem.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class PropertyPhotoConfiguration : IEntityTypeConfiguration<PropertyPhoto>
    {
        public void Configure(EntityTypeBuilder<PropertyPhoto> builder)
        {
            builder.ToTable("property_photos", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PhotoUrl)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.Caption)
                .HasMaxLength(255);

            builder.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Property)
                .WithMany(x => x.PropertyPhotos)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.PropertyId);
        }
    }
}
