using ApartmentManagementSystem.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Financial
{
    public class PaymentMatchConfiguration : IEntityTypeConfiguration<PaymentMatch>
    {
        public void Configure(EntityTypeBuilder<PaymentMatch> builder)
        {
            builder.ToTable("payment_matches", schema: "financial");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.MatchedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Payment)
                .WithMany(x => x.PaymentMatches)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.PaymentMatches)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            builder.HasIndex(x => new { x.PaymentId, x.InvoiceId }).IsUnique();

            // Indexes
            builder.HasIndex(x => x.PaymentId);
            builder.HasIndex(x => x.InvoiceId);
        }
    }
}
