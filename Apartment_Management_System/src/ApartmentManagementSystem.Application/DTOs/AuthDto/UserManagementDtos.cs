using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.AuthDto
{
    /// <summary>
    /// Row shape returned by GET /api/v1/users — light projection suitable for
    /// listing pages. Use <see cref="UserDetailsDto"/> for the per-record view.
    /// </summary>
    public class UserListItemDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string[] Roles { get; set; }
    }

    /// <summary>
    /// Full per-user payload for GET /api/v1/users/{id} — includes the
    /// flattened permission codes derived from the user's roles via the
    /// server-side Permissions matrix (Endpoints/Permissions.cs).
    /// </summary>
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string[] Roles { get; set; }
        public string[] Permissions { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Body for PUT /api/v1/users/{id}. Email is intentionally NOT editable —
    /// changing a user's email is a separate flow with re-verification.
    /// If <see cref="RoleNames"/> is supplied (non-null), it REPLACES the
    /// user's full role set; if null, roles are left untouched.
    /// </summary>
    public class UpdateUserRequest
    {
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? RoleNames { get; set; }
    }

    /// <summary>
    /// Query filters for GET /api/v1/users.
    /// </summary>
    public class ListUsersFilter
    {
        public string? Search { get; set; }
        public string? RoleName { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 50;
    }
}
