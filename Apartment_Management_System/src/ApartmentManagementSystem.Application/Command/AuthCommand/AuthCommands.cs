using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Infrastructure.Persistence;
using ApartmentManagementSystem.Application.DTOs.AuthDto;
using ApartmentManagementSystem.Application.Common.Interfaces;

namespace ApartmentManagementSystem.Application.Command.AuthCommand
{
    // Command: Register User
    public class RegisterUserCommand : IRequest<RegisterResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }  // Plain password - will be hashed by handler
        public string Phone { get; set; }
    }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ApartmentManagementSystem.Infrastructure.Security.IPasswordService _passwordService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            ApplicationDbContext context,
            ApartmentManagementSystem.Infrastructure.Security.IPasswordService passwordService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
                
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Email already registered"
                    };
                }

                // Hash password
                var passwordHash = _passwordService.HashPassword(request.Password);

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Phone = request.Phone,
                    IsActive = true,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                // Assign default tenant role
                var tenantRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "tenant", cancellationToken);
                
                if (tenantRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = tenantRole.Id
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("User registered successfully: {UserId}", user.Id);

                return new RegisterResponse
                {
                    Success = true,
                    UserId = user.Id,
                    Email = user.Email,
                    Message = "User registered successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return new RegisterResponse
                {
                    Success = false,
                    Message = $"Registration failed: {ex.GetBaseException().Message}"
                };
            }
        }
    }

    // Command: Create Refresh Token
    public class CreateRefreshTokenCommand : IRequest<int>
    {
        public int UserId { get; set; }
        public string TokenHash { get; set; }
    }

    public class CreateRefreshTokenCommandHandler : IRequestHandler<CreateRefreshTokenCommand, int>
    {
        private readonly ApplicationDbContext _context;

        public CreateRefreshTokenCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken
            {
                UserId = request.UserId,
                TokenHash = request.TokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            return refreshToken.Id;
        }
    }

    // Command: Revoke Refresh Token
    public class RevokeRefreshTokenCommand : IRequest<bool>
    {
        public int TokenId { get; set; }
    }

    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public RevokeRefreshTokenCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var token = await _context.RefreshTokens.FindAsync(new object[] { request.TokenId }, cancellationToken);
            if (token == null)
                return false;

            token.Revoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // Command: Create OTP
    public class CreateOtpCommand : IRequest<OtpResponseDto>
    {
        public int UserId { get; set; }
        public required string Phone { get; set; }
        public required string Code { get; set; }
        public required string Purpose { get; set; }
    }

    public class CreateOtpCommandHandler : IRequestHandler<CreateOtpCommand, OtpResponseDto>
    {
        private readonly ApplicationDbContext _context;

        public CreateOtpCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OtpResponseDto> Handle(CreateOtpCommand request, CancellationToken cancellationToken)
        {
            // Invalidate previous OTPs
            var previousOtps = await _context.OtpCodes
                .Where(o => o.Phone == request.Phone && o.UsedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var otp in previousOtps)
            {
                otp.UsedAt = DateTime.UtcNow;
                otp.UpdatedAt = DateTime.UtcNow;
            }

            var otpCode = new OtpCode
            {
                UserId = request.UserId,
                Phone = request.Phone,
                Code = request.Code,
                Purpose = request.Purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync(cancellationToken);

            return new OtpResponseDto
            {
                OtpId = otpCode.Id,
                Phone = otpCode.Phone,
                ExpiresAt = otpCode.ExpiresAt,
                Message = "OTP sent successfully"
            };
        }
    }

    // Command: Mark OTP Used
    public class MarkOtpUsedCommand : IRequest<bool>
    {
        public int OtpId { get; set; }
    }

    public class MarkOtpUsedCommandHandler : IRequestHandler<MarkOtpUsedCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public MarkOtpUsedCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkOtpUsedCommand request, CancellationToken cancellationToken)
        {
            var otp = await _context.OtpCodes.FindAsync(new object[] { request.OtpId }, cancellationToken);
            if (otp == null)
                return false;

            otp.UsedAt = DateTime.UtcNow;
            otp.UpdatedAt = DateTime.UtcNow;
            _context.OtpCodes.Update(otp);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // Command: Add Trusted Device
    public class AddTrustedDeviceCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }

    public class AddTrustedDeviceCommandHandler : IRequestHandler<AddTrustedDeviceCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public AddTrustedDeviceCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AddTrustedDeviceCommand request, CancellationToken cancellationToken)
        {
            var existingDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(td => td.UserId == request.UserId && td.DeviceId == request.DeviceId, cancellationToken);

            if (existingDevice != null)
            {
                existingDevice.TrustedUntil = DateTime.UtcNow.AddDays(30);
                existingDevice.UpdatedAt = DateTime.UtcNow;
                _context.TrustedDevices.Update(existingDevice);
            }
            else
            {
                var trustedDevice = new TrustedDevice
                {
                    UserId = request.UserId,
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName,
                    TrustedUntil = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TrustedDevices.Add(trustedDevice);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // Command: Update Last Login
    public class UpdateLastLoginCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    public class UpdateLastLoginCommandHandler : IRequestHandler<UpdateLastLoginCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public UpdateLastLoginCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateLastLoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
                return false;

            user.LastLoginAt = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // Command: Increment Failed Login Attempts
    public class IncrementFailedAttemptsCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    public class IncrementFailedAttemptsCommandHandler : IRequestHandler<IncrementFailedAttemptsCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public IncrementFailedAttemptsCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(IncrementFailedAttemptsCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
                return false;

            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    // Command: Revoke All Sessions
    public class RevokeAllSessionsCommand : IRequest<int>
    {
        public int UserId { get; set; }
    }

    public class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand, int>
    {
        private readonly ApplicationDbContext _context;

        public RevokeAllSessionsCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == request.UserId && !rt.Revoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.Revoked = true;
            }

            _context.RefreshTokens.UpdateRange(tokens);
            await _context.SaveChangesAsync(cancellationToken);
            return tokens.Count;
        }
    }

    // Command: Verify User Email
    public class VerifyUserEmailCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    public class VerifyUserEmailCommandHandler : IRequestHandler<VerifyUserEmailCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public VerifyUserEmailCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(VerifyUserEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
                return false;

            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
