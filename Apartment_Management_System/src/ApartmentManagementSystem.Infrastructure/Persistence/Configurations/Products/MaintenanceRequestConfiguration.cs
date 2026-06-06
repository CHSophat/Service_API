using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Products;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Products
{
    public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
        {
            builder.ToTable("maintenance_requests", schema: "product");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId);

            builder.Property(x => x.CustomerId);

            builder.Property(x => x.Priority)
                .HasDefaultValue("medium")
                .HasMaxLength(20);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.PhotoUrls)
                .HasColumnType("jsonb");

            builder.Property(x => x.Status)
                .HasDefaultValue("open")
                .HasMaxLength(20);

            builder.Property(x => x.AssignedTo);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Product)
                .WithMany(x => x.MaintenanceRequests)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Priority);
        }
    }
}
