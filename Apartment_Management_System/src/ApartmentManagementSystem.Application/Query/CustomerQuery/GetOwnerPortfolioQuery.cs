using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetOwnerPortfolioQuery : IRequest<OwnerPortfolioDto>
    {
        public int OwnerId { get; set; }
    }
}
