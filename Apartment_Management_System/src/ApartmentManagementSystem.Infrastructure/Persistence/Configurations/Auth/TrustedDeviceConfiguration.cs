using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Auth
{
    public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
    {
        public void Configure(EntityTypeBuilder<TrustedDevice> builder)
        {
            builder.ToTable("trusted_devices", schema: "auth");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId);

            builder.Property(x => x.DeviceId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.DeviceName)
                .HasMaxLength(255);

            builder.Property(x => x.TrustedUntil)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            builder.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
        }
    }
}
