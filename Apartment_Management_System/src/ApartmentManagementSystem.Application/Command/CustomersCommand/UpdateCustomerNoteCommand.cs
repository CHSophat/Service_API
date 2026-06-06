using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class UpdateCustomerNoteCommand : IRequest<CustomerNoteDto>
    {
        public int NoteId { get; set; }
        public string Content { get; set; }
    }
}
