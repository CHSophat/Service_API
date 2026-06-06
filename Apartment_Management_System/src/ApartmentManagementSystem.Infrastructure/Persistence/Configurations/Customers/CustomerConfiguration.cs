using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId);

            builder.Property(x => x.CustomerType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .HasMaxLength(255);

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.GovernmentId)
                .HasMaxLength(50)
                .HasAnnotation("Unique", true);

            builder.Property(x => x.ProfilePhotoUrl)
                .HasColumnType("text");

            builder.Property(x => x.EmergencyContactName)
                .HasMaxLength(200);

            builder.Property(x => x.EmergencyContactPhone)
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.Addresses)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Leases)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CommunicationLogs)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Notes)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId).IsUnique();
            builder.HasIndex(x => x.GovernmentId).IsUnique();
            builder.HasIndex(x => new { x.LastName, x.FirstName });
        }
    }
}
