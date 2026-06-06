using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
    {
        public void Configure(EntityTypeBuilder<ProductPhoto> builder)
        {
            builder.ToTable("product_photos", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId);

            builder.Property(x => x.PhotoUrl)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // product_photos table has no updated_at column (see
            // scripts/04_product_tables.sql). UpdatedAt is inherited from
            // AuditableEntity and must be ignored, otherwise EF emits
            // p.updated_at and Postgres returns 42703 column-not-found.
            builder.Ignore(x => x.UpdatedAt);

            // Relationships
            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductPhotos)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.ProductId);
        }
    }
}
