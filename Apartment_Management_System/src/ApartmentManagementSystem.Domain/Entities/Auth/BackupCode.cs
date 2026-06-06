using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Auth
{
    public class BackupCode : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CodeHash { get; set; }
        public DateTime? UsedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}
