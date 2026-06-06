using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class CustomerProfileDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string GovernmentId { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public EmergencyContactDto EmergencyContact { get; set; }
        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        public LeaseDto CurrentLease { get; set; }
        public int TotalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
