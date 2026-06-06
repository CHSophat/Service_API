using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasAnnotation("Unique", true);

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .HasDefaultValue("vacant")
                .HasMaxLength(20);

            builder.Property(x => x.BasePrice)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.Bathrooms)
                .HasColumnType("decimal(3,1)");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.ProductAttributes)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ProductPhotos)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.MaintenanceRequests)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => new { x.ProductType, x.Status });
            builder.HasIndex(x => x.BasePrice);
        }
    }
}
