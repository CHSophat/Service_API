using ApartmentManagementSystem.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Financial
{
    public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
    {
        public void Configure(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder.ToTable("invoice_lines", schema: "financial");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LineType)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Quantity)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(1);

            builder.Property(x => x.UnitPrice)
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.LineTotal)
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.InvoiceLines)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.InvoiceId);
        }
    }
}
