using System.Collections.Generic;

namespace ApartmentManagementSystem.Domain.Entities.Auth
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
