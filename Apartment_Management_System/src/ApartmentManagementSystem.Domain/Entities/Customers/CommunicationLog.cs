using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Customers
{
    public class CommunicationLog : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; } // email, sms, ticket
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Direction { get; set; } // inbound, outbound

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
}
