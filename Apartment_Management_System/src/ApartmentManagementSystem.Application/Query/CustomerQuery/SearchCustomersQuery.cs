using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class SearchCustomersQuery : IRequest<List<CustomerProfileDto>>
    {
        public required string SearchTerm { get; set; }
    }
}
