using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ApartmentManagementSystem.Application.Query.Repositories;
using ApartmentManagementSystem.Application.DTOs.AuthDto;

namespace ApartmentManagementSystem.Application.Query.AuthQuery
{
    // Query: Get Current User
    public class GetCurrentUserQuery : IRequest<UserProfileResponse?>
    {
        public int UserId { get; set; }
    }

    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileResponse?>
    {
        private readonly IAuthRepository _authRepository;

        public GetCurrentUserQueryHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<UserProfileResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _authRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                return null;

            var roles = await _authRepository.GetUserRolesAsync(user.Id);
            var roleNames = new List<string>();
            foreach (var role in roles)
                roleNames.Add(role.Name);

            return new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                EmailVerifiedAt = user.EmailVerifiedAt,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                Is2FaEnabled = false, // TODO: Add 2FA field to User entity
                Roles = roleNames.ToArray()
            };
        }
    }

    // Query: Get Active Sessions
    public class GetActiveSessionsQuery : IRequest<ActiveSessionsResponse>
    {
        public int UserId { get; set; }
        public string CurrentTokenHash { get; set; }
    }

    public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, ActiveSessionsResponse>
    {
        private readonly IAuthRepository _authRepository;

        public GetActiveSessionsQueryHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<ActiveSessionsResponse> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
        {
            var sessions = await _authRepository.GetActiveSessionsAsync(request.UserId);
            var response = new ActiveSessionsResponse
            {
                Sessions = new List<SessionResponse>(),
                TotalActiveSessions = sessions.Count
            };

            foreach (var session in sessions)
            {
                response.Sessions.Add(new SessionResponse
                {
                    SessionId = session.Id,
                    CreatedAt = session.CreatedAt,
                    ExpiresAt = session.ExpiresAt,
                    IsCurrent = session.TokenHash == request.CurrentTokenHash
                });
            }

            return response;
        }
    }

    // Query: Get User By Email
    public class GetUserByEmailQuery : IRequest<UserAuthDto>
    {
        public string Email { get; set; }
    }

    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserAuthDto>
    {
        private readonly IAuthRepository _authRepository;

        public GetUserByEmailQueryHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<UserAuthDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return null;

            var roles = await _authRepository.GetUserRolesAsync(user.Id);
            var roleNames = new List<string>();
            foreach (var role in roles)
                roleNames.Add(role.Name);

            return new UserAuthDto
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                PasswordHash = user.PasswordHash,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                EmailVerifiedAt = user.EmailVerifiedAt,
                LastLoginAt = user.LastLoginAt,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockedUntil = user.LockedUntil,
                Is2FaEnabled = false,
                Roles = roleNames
            };
        }
    }

    // Query: Check User Exists By Email
    public class CheckUserExistsByEmailQuery : IRequest<bool>
    {
        public string Email { get; set; }
    }

    public class CheckUserExistsByEmailQueryHandler : IRequestHandler<CheckUserExistsByEmailQuery, bool>
    {
        private readonly IAuthRepository _authRepository;

        public CheckUserExistsByEmailQueryHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<bool> Handle(CheckUserExistsByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _authRepository.UserExistsByEmailAsync(request.Email);
        }
    }
}
