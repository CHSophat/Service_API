using MediatR;

namespace ApartmentManagementSystem.Application.Command.MessagingCommand
{
    public class MarkConversationReadCommand : IRequest<bool>
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
    }
}
