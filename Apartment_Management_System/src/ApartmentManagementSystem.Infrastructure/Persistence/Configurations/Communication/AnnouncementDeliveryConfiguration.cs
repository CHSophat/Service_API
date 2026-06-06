using ApartmentManagementSystem.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Communication
{
    public class AnnouncementDeliveryConfiguration : IEntityTypeConfiguration<AnnouncementDelivery>
    {
        public void Configure(EntityTypeBuilder<AnnouncementDelivery> builder)
        {
            builder.ToTable("announcement_deliveries", schema: "communication");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeliveredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Announcement)
                .WithMany(x => x.AnnouncementDeliveries)
                .HasForeignKey(x => x.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.AnnouncementDeliveries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            builder.HasIndex(x => new { x.AnnouncementId, x.UserId }).IsUnique();

            // Indexes
            builder.HasIndex(x => x.UserId);
        }
    }
}
