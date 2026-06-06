using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetCustomerProfileQuery : IRequest<CustomerProfileDto>
    {
        public int CustomerId { get; set; }
    }
}
