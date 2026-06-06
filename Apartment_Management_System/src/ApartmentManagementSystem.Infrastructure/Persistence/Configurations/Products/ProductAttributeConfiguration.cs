using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("product_attributes", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId);

            builder.Property(x => x.AttributeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.AttributeValue)
                .IsRequired()
                .HasColumnType("text");

            // product_attributes table has no created_at / updated_at columns
            // in the schema (see scripts/04_product_tables.sql). The entity
            // inherits these from AuditableEntity, so they must be explicitly
            // ignored or EF emits SELECT p.created_at and Postgres returns
            // 42703 column-not-found on every Include(ProductAttributes).
            builder.Ignore(x => x.CreatedAt);
            builder.Ignore(x => x.UpdatedAt);

            // Relationships
            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductAttributes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.ProductId);
        }
    }
}
