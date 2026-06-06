using System;
using MediatR;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class DeleteCustomerNoteCommand : IRequest<bool>
    {
        public int NoteId { get; set; }
    }
}
