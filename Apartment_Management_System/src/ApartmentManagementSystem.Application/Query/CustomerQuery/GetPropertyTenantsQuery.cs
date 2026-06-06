using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetPropertyTenantsQuery : IRequest<List<CustomerProfileDto>>
    {
        public int PropertyId { get; set; }
    }
}
