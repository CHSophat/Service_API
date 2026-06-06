using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Query.CustomerQuery
{
    public class GetMoveChecklistQuery : IRequest<MoveChecklistDto>
    {
        public int ChecklistId { get; set; }
    }
}
