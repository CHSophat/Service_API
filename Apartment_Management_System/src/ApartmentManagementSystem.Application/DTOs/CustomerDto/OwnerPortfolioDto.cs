using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.CustomerDto
{
    public class OwnerPortfolioDto
    {
        public int OwnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerType { get; set; }
        public List<OwnedPropertyDto> OwnedProperties { get; set; } = new List<OwnedPropertyDto>();
        public PaymentSummaryDto PaymentSummary { get; set; }
    }

    public class OwnedPropertyDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string Address { get; set; }
        public decimal CurrentRent { get; set; }
        public string LeaseStatus { get; set; }
        public TenantDto CurrentTenant { get; set; }
    }

    public class TenantDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantPhone { get; set; }
    }

    public class PaymentSummaryDto
    {
        public decimal TotalReceived { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int UpcomingPayments { get; set; }
    }
}
