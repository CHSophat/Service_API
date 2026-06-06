using ApartmentManagementSystem.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentManagementSystem.Infrastructure.Persistence.Configurations.Communication
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages", schema: "communication");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Kind)
                .HasDefaultValue("text")
                .HasMaxLength(20);

            builder.Property(x => x.Body)
                .HasColumnType("text");

            builder.Property(x => x.AttachmentUrl)
                .HasColumnType("text");

            builder.Property(x => x.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SenderUser)
                .WithMany(x => x.SentMessages)
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => new { x.ConversationId, x.SentAt });
            builder.HasIndex(x => x.SenderUserId);
        }
    }
}
