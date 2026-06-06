using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class CreateMoveChecklistCommand : IRequest<MoveChecklistDto>
    {
        public int LeaseId { get; set; }
        public required string ChecklistType { get; set; } // move_in, move_out
        public DateTime InspectionDate { get; set; }
        public required string InspectorName { get; set; }
        public required string OverallCondition { get; set; }
        public required string Notes { get; set; }
    }
}
