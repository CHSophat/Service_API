using System;
using System.Collections.Generic;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetLeaseDocumentsQuery : IRequest<List<LeaseDocumentDto>>
    {
        public int LeaseId { get; set; }
    }
}
