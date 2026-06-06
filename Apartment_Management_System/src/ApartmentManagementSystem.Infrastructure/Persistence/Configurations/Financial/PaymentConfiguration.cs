using ApartmentManagementSystem.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Financial
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments", schema: "financial");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MethodKind)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.Currency)
                .HasDefaultValue("USD")
                .HasMaxLength(3);

            builder.Property(x => x.Status)
                .HasDefaultValue("pending")
                .HasMaxLength(20);

            builder.Property(x => x.BakongQrString)
                .HasColumnType("text");

            builder.Property(x => x.BakongMd5)
                .HasMaxLength(64);

            builder.Property(x => x.ExternalRef)
                .HasMaxLength(100);

            builder.Property(x => x.ReceiptUrl)
                .HasColumnType("text");

            builder.Property(x => x.Notes)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.PaymentMatches)
                .WithOne(x => x.Payment)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PaidAt);
        }
    }
}
