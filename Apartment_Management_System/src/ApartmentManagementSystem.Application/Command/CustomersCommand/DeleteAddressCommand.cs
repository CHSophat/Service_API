using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class DeleteAddressCommand : IRequest<bool>
    {
        public int AddressId { get; set; }
    }
}
