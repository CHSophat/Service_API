using System.Collections.Generic;
using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.MessagingCommand
{
    public class CreateConversationCommand : IRequest<ConversationDto>
    {
        public int CreatedByUserId { get; set; }
        public string? Subject { get; set; }
        public int? PropertyId { get; set; }
        public List<int> ParticipantUserIds { get; set; } = new List<int>();
    }
}
