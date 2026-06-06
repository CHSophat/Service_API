using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class LeaseDocumentDto
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime? SignedDate { get; set; }
        public string SignedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
