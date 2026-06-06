using ApartmentManagementSystem.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class PropertyOwnerConfiguration : IEntityTypeConfiguration<PropertyOwner>
    {
        public void Configure(EntityTypeBuilder<PropertyOwner> builder)
        {
            builder.ToTable("property_owners", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OwnershipPct)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(100.00m);

            builder.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Property)
                .WithMany(x => x.PropertyOwners)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.PropertyOwners)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            builder.HasIndex(x => new { x.PropertyId, x.CustomerId }).IsUnique();

            // Indexes
            builder.HasIndex(x => x.PropertyId);
            builder.HasIndex(x => x.CustomerId);
        }
    }
}
