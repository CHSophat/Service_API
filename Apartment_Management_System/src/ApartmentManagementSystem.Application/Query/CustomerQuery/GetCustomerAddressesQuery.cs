using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetCustomerAddressesQuery : IRequest<List<AddressDto>>
    {
        public int CustomerId { get; set; }
    }
}
