using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class SignLeaseCommand : IRequest<bool>
    {
        public int LeaseId { get; set; }
        public string SignedBy { get; set; }
    }
}
