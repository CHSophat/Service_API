using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using ApartmentManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagementSystem.Application.Query.MessagingQuery
{
    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetConversationsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _context.Conversations
                .Where(c => c.ConversationParticipants.Any(p => p.UserId == request.UserId))
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Subject = c.Subject,
                    PropertyId = c.PropertyId,
                    CreatedBy = c.CreatedBy,
                    LastMessageAt = c.LastMessageAt,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return conversations;
        }
    }

    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto?>
    {
        private readonly ApplicationDbContext _context;

        public GetConversationByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationDto?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(p => p.ConversationId == request.Id && p.UserId == request.UserId, cancellationToken);

            if (!isParticipant)
                return null;

            var conversation = await _context.Conversations
                .Where(c => c.Id == request.Id)
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Subject = c.Subject,
                    PropertyId = c.PropertyId,
                    CreatedBy = c.CreatedBy,
                    LastMessageAt = c.LastMessageAt,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return conversation;
        }
    }

    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetMessagesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);

            if (!isParticipant)
                return new List<MessageDto>();

            var query = _context.Messages
                .Where(m => m.ConversationId == request.ConversationId && m.DeletedAt == null);

            if (request.Before.HasValue)
            {
                query = query.Where(m => m.SentAt < request.Before.Value);
            }

            var messages = await query
                .OrderByDescending(m => m.SentAt)
                .Take(request.Limit)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderUserId = m.SenderUserId,
                    Kind = m.Kind,
                    Body = m.Body,
                    AttachmentUrl = m.AttachmentUrl,
                    SentAt = m.SentAt,
                    EditedAt = m.EditedAt,
                    DeletedAt = m.DeletedAt
                })
                .ToListAsync(cancellationToken);

            // Reorder chronologically for frontend displaying
            messages.Reverse();
            return messages;
        }
    }
}
