using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetCommunicationsQuery : IRequest<List<CommunicationLogDto>>
    {
        public int CustomerId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Type { get; set; }
        public string? Direction { get; set; }
    }
}
