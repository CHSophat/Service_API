using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("addresses", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CustomerId);

            builder.Property(x => x.AddressType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Street)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .HasMaxLength(50);

            builder.Property(x => x.PostalCode)
                .HasMaxLength(20);

            builder.Property(x => x.Country)
                .HasMaxLength(50);

            builder.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CustomerId);
        }
    }
}
