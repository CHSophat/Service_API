using System;
using MediatR;
using ApartmentManagementSystem.Application.DTOs.CustomerDto;

namespace ApartmentManagementSystem.Application.Command.CustomersCommand
{
    public class CreateCustomerCommand : IRequest<CustomerProfileDto>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string CustomerType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public required string GovernmentId { get; set; }
        public required string EmergencyContactName { get; set; }
        public required string EmergencyContactPhone { get; set; }
    }
}
