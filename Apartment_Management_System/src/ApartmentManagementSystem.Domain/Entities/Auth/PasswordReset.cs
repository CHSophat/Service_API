using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Auth
{
    public class PasswordReset : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ResetToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}
