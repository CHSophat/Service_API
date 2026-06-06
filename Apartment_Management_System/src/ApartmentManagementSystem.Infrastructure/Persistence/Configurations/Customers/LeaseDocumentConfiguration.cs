using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class LeaseDocumentConfiguration : IEntityTypeConfiguration<LeaseDocument>
    {
        public void Configure(EntityTypeBuilder<LeaseDocument> builder)
        {
            builder.ToTable("lease_documents", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LeaseId)
                .IsRequired();

            builder.Property(x => x.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.FileUrl)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending_review");

            builder.Property(x => x.Description)
                .HasColumnType("text");

            builder.Property(x => x.SignedBy)
                .HasMaxLength(200);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Lease)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.LeaseId);
        }
    }
}
