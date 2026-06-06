using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using ApartmentManagementSystem.Domain.Entities.Communication;
using ApartmentManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagementSystem.Application.Command.MessagingCommand
{
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
    {
        private readonly ApplicationDbContext _context;

        public CreateConversationCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            var conversation = new Conversation
            {
                Subject = request.Subject,
                PropertyId = request.PropertyId,
                CreatedBy = request.CreatedByUserId,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            // Add participants
            var participants = new List<ConversationParticipant>
            {
                new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    UserId = request.CreatedByUserId,
                    JoinedAt = DateTime.UtcNow
                }
            };

            foreach (var userId in request.ParticipantUserIds.Distinct())
            {
                if (userId != request.CreatedByUserId)
                {
                    participants.Add(new ConversationParticipant
                    {
                        ConversationId = conversation.Id,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            _context.ConversationParticipants.AddRange(participants);
            await _context.SaveChangesAsync(cancellationToken);

            return new ConversationDto
            {
                Id = conversation.Id,
                Subject = conversation.Subject,
                PropertyId = conversation.PropertyId,
                CreatedBy = conversation.CreatedBy,
                LastMessageAt = conversation.LastMessageAt,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt
            };
        }
    }

    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto?>
    {
        private readonly ApplicationDbContext _context;

        public SendMessageCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MessageDto?> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.SenderUserId, cancellationToken);

            if (!isParticipant)
                return null;

            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderUserId = request.SenderUserId,
                Kind = request.Kind,
                Body = request.Body,
                AttachmentUrl = request.AttachmentUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Update conversation's last message time
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);
            if (conversation != null)
            {
                conversation.LastMessageAt = message.SentAt;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                Kind = message.Kind,
                Body = message.Body,
                AttachmentUrl = message.AttachmentUrl,
                SentAt = message.SentAt,
                EditedAt = message.EditedAt,
                DeletedAt = message.DeletedAt
            };
        }
    }

    public class MarkConversationReadCommandHandler : IRequestHandler<MarkConversationReadCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public MarkConversationReadCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);

            if (participant == null)
                return false;

            participant.LastReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
