using System;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class LeaseDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public string Status { get; set; }
        public string SignedDocumentUrl { get; set; }
        public DateTime? SignedDate { get; set; }
        public int ProductId { get; set; }
    }
}
