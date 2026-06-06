using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Application.DTOs.AuthDto;
using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Command.UserCommand
{
    // ============================================================================
    // Update — phone + IsActive + role replacement.
    //
    // Email changes deliberately live in a separate flow (re-verify required) so
    // we don't accept Email on this command. Roles are REPLACED when RoleNames
    // is non-null; passing null leaves roles untouched. Unknown role names are
    // silently dropped — the response shows the actual saved set.
    // ============================================================================

    public class UpdateUserCommand : IRequest<UserDetailsDto?>
    {
        public int UserId { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? RoleNames { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDetailsDto?>
    {
        private readonly ApplicationDbContext _context;

        public UpdateUserCommandHandler(ApplicationDbContext context) => _context = context;

        public async Task<UserDetailsDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null) return null;

            if (request.Phone is not null) user.Phone = request.Phone;
            if (request.IsActive.HasValue)  user.IsActive = request.IsActive.Value;

            if (request.RoleNames is not null)
            {
                var desired = request.RoleNames
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var roleEntities = await _context.Roles
                    .Where(r => desired.Contains(r.Name))
                    .ToListAsync(cancellationToken);

                var current = user.UserRoles.ToList();
                foreach (var ur in current)
                {
                    if (!roleEntities.Any(r => r.Id == ur.RoleId))
                        _context.UserRoles.Remove(ur);
                }

                foreach (var role in roleEntities)
                {
                    if (!current.Any(ur => ur.RoleId == role.Id))
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                    }
                }
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Re-read with the new role set so the response reflects what was saved.
            var fresh = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstAsync(u => u.Id == user.Id, cancellationToken);

            return new UserDetailsDto
            {
                Id = fresh.Id,
                Email = fresh.Email,
                Phone = fresh.Phone,
                IsActive = fresh.IsActive,
                IsEmailVerified = fresh.IsEmailVerified,
                EmailVerifiedAt = fresh.EmailVerifiedAt,
                LastLoginAt = fresh.LastLoginAt,
                FailedLoginAttempts = fresh.FailedLoginAttempts,
                LockedUntil = fresh.LockedUntil,
                CreatedAt = fresh.CreatedAt,
                UpdatedAt = fresh.UpdatedAt,
                Roles = fresh.UserRoles.Select(ur => ur.Role.Name).ToArray()
            };
        }
    }

    // ============================================================================
    // Delete — soft-delete via IsActive=false plus revocation of refresh tokens.
    //
    // Hard-deleting a user with active foreign keys (AuditLogs, Messages,
    // ConversationParticipants, AnnouncementDeliveries, RefreshTokens) would
    // either cascade through audit history (bad) or violate FK constraints
    // (worse). Setting IsActive=false matches what the login flow already
    // checks (AccountDisabled) and is reversible by re-toggling IsActive.
    // ============================================================================

    public class DeleteUserCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteUserCommandHandler(ApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            // Force the user off every active session.
            foreach (var token in user.RefreshTokens)
            {
                if (!token.Revoked) token.Revoked = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
