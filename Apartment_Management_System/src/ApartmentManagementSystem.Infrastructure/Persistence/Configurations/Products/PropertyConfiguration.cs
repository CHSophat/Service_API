using ApartmentManagementSystem.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            builder.ToTable("properties", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasAnnotation("Unique", true);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasColumnType("text");

            builder.Property(x => x.AddressLine)
                .HasMaxLength(255);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .HasMaxLength(50);

            builder.Property(x => x.PostalCode)
                .HasMaxLength(20);

            builder.Property(x => x.Country)
                .HasDefaultValue("KH")
                .HasMaxLength(50);

            builder.Property(x => x.Latitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(x => x.Longitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(x => x.TotalUnits)
                .HasDefaultValue(0);

            builder.Property(x => x.Status)
                .HasDefaultValue("active")
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.Products)
                .WithOne(x => x.Property)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.PropertyOwners)
                .WithOne(x => x.Property)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.PropertyPhotos)
                .WithOne(x => x.Property)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.City);
        }
    }
}
