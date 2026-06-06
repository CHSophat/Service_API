using ApartmentManagementSystem.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Financial
{
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("payment_methods", schema: "financial");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Kind)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Label)
                .HasMaxLength(100);

            builder.Property(x => x.Masked)
                .HasMaxLength(50);

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.Property(x => x.Metadata)
                .HasColumnType("jsonb");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.PaymentMethods)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CustomerId);
        }
    }
}
