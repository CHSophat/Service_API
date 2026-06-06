using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
    {
        public void Configure(EntityTypeBuilder<ChecklistItem> builder)
        {
            builder.ToTable("checklist_items", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ChecklistId)
                .IsRequired();

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Condition)
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .HasColumnType("text");

            builder.Property(x => x.IsDamage)
                .HasDefaultValue(false);

            builder.Property(x => x.PhotoUrls)
                .HasColumnType("jsonb");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Checklist)
                .WithMany(x => x.ChecklistItems)
                .HasForeignKey(x => x.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.ChecklistId);
        }
    }
}
