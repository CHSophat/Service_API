using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Auth
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", schema: "auth");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email")
                .HasAnnotation("Unique", true);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            builder.Property(x => x.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            builder.Property(x => x.IsEmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_email_verified");

            builder.Property(x => x.FailedLoginAttempts)
                .HasDefaultValue(0)
                .HasColumnName("failed_login_attempts");

            builder.Property(x => x.EmailVerifiedAt)
                .HasColumnName("email_verified_at");

            builder.Property(x => x.LastLoginAt)
                .HasColumnName("last_login_at");

            builder.Property(x => x.LockedUntil)
                .HasColumnName("locked_until");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            // Relationships
            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.AuditLogs)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.IsActive);
        }
    }
}
