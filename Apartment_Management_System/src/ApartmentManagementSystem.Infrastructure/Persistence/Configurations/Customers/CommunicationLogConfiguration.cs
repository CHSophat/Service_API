using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
    {
        public void Configure(EntityTypeBuilder<CommunicationLog> builder)
        {
            builder.ToTable("communications_log", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CustomerId);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Subject)
                .HasMaxLength(255);

            builder.Property(x => x.Message)
                .HasColumnType("text");

            builder.Property(x => x.Direction)
                .HasMaxLength(10);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.CommunicationLogs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CustomerId);
        }
    }
}
