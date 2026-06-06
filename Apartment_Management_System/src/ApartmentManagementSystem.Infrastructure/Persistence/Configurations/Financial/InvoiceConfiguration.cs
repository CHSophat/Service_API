using ApartmentManagementSystem.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Financial
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("invoices", schema: "financial");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasAnnotation("Unique", true);

            builder.Property(x => x.Status)
                .HasDefaultValue("draft")
                .HasMaxLength(20);

            builder.Property(x => x.Currency)
                .HasDefaultValue("USD")
                .HasMaxLength(3);

            builder.Property(x => x.Subtotal)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.Tax)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.Total)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.AmountPaid)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.Notes)
                .HasColumnType("text");

            builder.Property(x => x.PdfUrl)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.InvoiceLines)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.PaymentMatches)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.InvoiceNumber).IsUnique();
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.PropertyId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.DueDate);
            builder.HasIndex(x => new { x.PeriodStart, x.PeriodEnd });
        }
    }
}
