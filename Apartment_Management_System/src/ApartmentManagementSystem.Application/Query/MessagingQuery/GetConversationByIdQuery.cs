using ApartmentManagementSystem.Application.DTOs.MessagingDto;
using MediatR;

namespace ApartmentManagementSystem.Application.Query.MessagingQuery
{
    public class GetConversationByIdQuery : IRequest<ConversationDto?>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
