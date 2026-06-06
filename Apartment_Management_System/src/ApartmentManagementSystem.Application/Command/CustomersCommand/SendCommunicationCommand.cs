using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class SendCommunicationCommand : IRequest<int>
    {
        public int CustomerId { get; set; }
        public string Type { get; set; } // email, sms, ticket
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Direction { get; set; } // inbound, outbound
    }
}
