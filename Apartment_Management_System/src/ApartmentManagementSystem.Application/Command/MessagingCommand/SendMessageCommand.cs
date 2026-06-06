using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.MessagingCommand
{
    public class SendMessageCommand : IRequest<MessageDto?>
    {
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string Body { get; set; } = null!;
        public string Kind { get; set; } = "text";
        public string? AttachmentUrl { get; set; }
    }
}
