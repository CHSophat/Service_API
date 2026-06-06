using ApartmentManagementSystem.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Communication
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("announcements", schema: "communication");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Body)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.Audience)
                .HasDefaultValue("all")
                .HasMaxLength(30);

            builder.Property(x => x.Status)
                .HasDefaultValue("draft")
                .HasMaxLength(20);

            builder.Property(x => x.CoverUrl)
                .HasColumnType("text");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasMany(x => x.AnnouncementDeliveries)
                .WithOne(x => x.Announcement)
                .HasForeignKey(x => x.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Audience);
            builder.HasIndex(x => x.PropertyId);
        }
    }
}
