using System;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class CommunicationLogDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Direction { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; }
        public CommunicationMetadataDto Metadata { get; set; }
    }

    public class CommunicationMetadataDto
    {
        public bool HasResponse { get; set; }
        public double? ResponseTimeHours { get; set; }
    }
}
