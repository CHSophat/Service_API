using System;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class CustomerNoteDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
