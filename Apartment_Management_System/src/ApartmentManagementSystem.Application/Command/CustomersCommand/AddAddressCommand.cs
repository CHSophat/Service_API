using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class AddAddressCommand : IRequest<int>
    {
        public int CustomerId { get; set; }
        public required string AddressType { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public bool IsPrimary { get; set; }
    }
}
