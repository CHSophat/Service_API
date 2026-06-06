using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetAllCustomersQuery : IRequest<List<CustomerProfileDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
