using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class AddCustomerNoteCommand : IRequest<CustomerNoteDto>
    {
        public int CustomerId { get; set; }
        public required string Content { get; set; }
        public required string CreatedBy { get; set; }
    }
}
