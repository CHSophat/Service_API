using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Customers;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Customers
{
    public class MoveChecklistConfiguration : IEntityTypeConfiguration<MoveChecklist>
    {
        public void Configure(EntityTypeBuilder<MoveChecklist> builder)
        {
            builder.ToTable("move_checklists", schema: "customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LeaseId)
                .IsRequired();

            builder.Property(x => x.ChecklistType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.InspectorName)
                .HasMaxLength(200);

            builder.Property(x => x.OverallCondition)
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Lease)
                .WithMany(x => x.Checklists)
                .HasForeignKey(x => x.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ChecklistItems)
                .WithOne(x => x.Checklist)
                .HasForeignKey(x => x.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.LeaseId);
            builder.HasIndex(x => new { x.LeaseId, x.ChecklistType }).IsUnique();
        }
    }
}
