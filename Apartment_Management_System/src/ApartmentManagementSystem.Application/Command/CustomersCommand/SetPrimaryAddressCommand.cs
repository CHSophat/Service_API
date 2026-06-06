using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class SetPrimaryAddressCommand : IRequest<bool>
    {
        public int AddressId { get; set; }
    }
}
