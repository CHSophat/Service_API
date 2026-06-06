using System;
using ApartmentManagementSystem.Domain.Entities.Base;

namespace ApartmentManagementSystem.Domain.Entities.Auth
{
    public class OtpCode : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
        public string? Purpose { get; set; } = "login"; // login, password_reset, email_verification
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int Attempts { get; set; } = 0;

        // Navigation properties
        public virtual User User { get; set; }
    }
}
