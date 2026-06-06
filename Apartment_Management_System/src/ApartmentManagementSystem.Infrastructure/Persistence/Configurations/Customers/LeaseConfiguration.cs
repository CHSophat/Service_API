using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
    {
        public void Configure(EntityTypeBuilder<Lease> builder)
        {
            builder.ToTable("leases", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CustomerId);

            builder.Property(x => x.ProductId);

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.MonthlyRent)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.SecurityDeposit)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.Status)
                .HasDefaultValue("active")
                .HasMaxLength(20);

            builder.Property(x => x.SignedDocumentUrl)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Leases)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => new { x.StartDate, x.EndDate });
            builder.HasIndex(x => x.Status);
        }
    }
}
