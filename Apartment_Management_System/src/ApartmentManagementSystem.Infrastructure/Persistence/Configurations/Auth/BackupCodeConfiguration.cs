using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Auth
{
    public class BackupCodeConfiguration : IEntityTypeConfiguration<BackupCode>
    {
        public void Configure(EntityTypeBuilder<BackupCode> builder)
        {
            builder.ToTable("backup_codes", schema: "auth");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId);

            builder.Property(x => x.CodeHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId);
        }
    }
}
