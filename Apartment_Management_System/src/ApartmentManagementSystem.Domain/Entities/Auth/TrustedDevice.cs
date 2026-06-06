using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Auth
{
    public class TrustedDevice : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DateTime TrustedUntil { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}
