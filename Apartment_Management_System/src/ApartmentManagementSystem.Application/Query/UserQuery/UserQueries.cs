using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.AuthDto;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.UserQuery
{
    // ============================================================================
    // List users — projection used by Settings → Users tab.
    // ============================================================================

    public class ListUsersQuery : IRequest<List<UserListItemDto>>
    {
        public string? Search { get; set; }
        public string? RoleName { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 50;
    }

    public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, List<UserListItemDto>>
    {
        private readonly ApplicationDbContext _context;

        public ListUsersQueryHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<UserListItemDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            var page  = request.Page  <= 0 ? 1 : request.Page;
            var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 200);

            var query = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim();
                query = query.Where(u =>
                    EF.Functions.Like(u.Email, $"%{s}%") ||
                    EF.Functions.Like(u.Phone ?? string.Empty, $"%{s}%"));
            }

            if (request.IsActive.HasValue)
            {
                var on = request.IsActive.Value;
                query = query.Where(u => u.IsActive == on);
            }

            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                var role = request.RoleName.Trim();
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
            }

            var rows = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToArray()
                })
                .ToListAsync(cancellationToken);

            return rows;
        }
    }

    // ============================================================================
    // Get one user — full payload. Permissions are computed in the endpoint
    // layer (see UserEndpoints) so the matrix stays in one place.
    // ============================================================================

    public class GetUserByIdQuery : IRequest<UserDetailsDto?>
    {
        public int UserId { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailsDto?>
    {
        private readonly ApplicationDbContext _context;

        public GetUserByIdQueryHandler(ApplicationDbContext context) => _context = context;

        public async Task<UserDetailsDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null) return null;

            return new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                EmailVerifiedAt = user.EmailVerifiedAt,
                LastLoginAt = user.LastLoginAt,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockedUntil = user.LockedUntil,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray()
            };
        }
    }
}
