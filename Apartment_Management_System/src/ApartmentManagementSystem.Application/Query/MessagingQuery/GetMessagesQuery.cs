using System;
using System.Collections.Generic;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;

namespace ApartmentManagementSystem.Application.Query.MessagingQuery
{
    public class GetMessagesQuery : IRequest<List<MessageDto>>
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public DateTime? Before { get; set; }
        public int Limit { get; set; } = 50;
    }
}
