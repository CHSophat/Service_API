using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class UpdateAddressCommand : IRequest<bool>
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsPrimary { get; set; }
    }
}
