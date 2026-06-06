using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class UploadLeaseDocumentCommand : IRequest<LeaseDocumentDto>
    {
        public int LeaseId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Description { get; set; }
    }
}
