using System;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class CreateCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerType { get; set; } // tenant, owner, both
        public DateTime? DateOfBirth { get; set; }
        public string GovernmentId { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
    }
}
