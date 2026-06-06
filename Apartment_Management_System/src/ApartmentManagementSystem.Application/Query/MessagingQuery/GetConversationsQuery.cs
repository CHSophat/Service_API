using System.Collections.Generic;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;

namespace ApartmentManagementSystem.Application.Query.MessagingQuery
{
    public class GetConversationsQuery : IRequest<List<ConversationDto>>
    {
        public int UserId { get; set; }
    }
}
