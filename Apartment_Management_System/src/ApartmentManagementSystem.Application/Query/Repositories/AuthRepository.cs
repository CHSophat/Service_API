using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Infrastructure.Persistence;

namespace ApartmentManagementSystem.Application.Query.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // User queries
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByPhoneAsync(string phone)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByPhoneAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        // Role queries
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
        }

        // Refresh token queries
        public async Task<RefreshToken> GetRefreshTokenByIdAsync(int tokenId)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == tokenId);
        }

        public async Task<RefreshToken> GetRefreshTokenByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<List<RefreshToken>> GetActiveSessionsAsync(int userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.Revoked && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountActiveSessionsAsync(int userId)
        {
            return await _context.RefreshTokens
                .CountAsync(rt => rt.UserId == userId && !rt.Revoked && rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<bool> IsTokenRevokedAsync(string tokenHash)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
            return token?.Revoked ?? true;
        }

        // OTP queries
        public async Task<OtpCode> GetValidOtpAsync(string phone, string code)
        {
            return await _context.OtpCodes
                .Where(oc => oc.Phone == phone 
                    && oc.Code == code 
                    && oc.UsedAt == null 
                    && oc.ExpiresAt > DateTime.UtcNow
                    && oc.Attempts < 3)
                .OrderByDescending(oc => oc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpCode> GetOtpByIdAsync(int otpId)
        {
            return await _context.OtpCodes
                .FirstOrDefaultAsync(oc => oc.Id == otpId);
        }

        public async Task<bool> HasUnusedOtpAsync(string phone)
        {
            return await _context.OtpCodes
                .AnyAsync(oc => oc.Phone == phone 
                    && oc.UsedAt == null 
                    && oc.ExpiresAt > DateTime.UtcNow);
        }

        // Trusted device queries
        public async Task<TrustedDevice> GetTrustedDeviceAsync(int userId, string deviceId)
        {
            return await _context.TrustedDevices
                .FirstOrDefaultAsync(td => td.UserId == userId && td.DeviceId == deviceId);
        }

        public async Task<List<TrustedDevice>> GetTrustedDevicesAsync(int userId)
        {
            return await _context.TrustedDevices
                .Where(td => td.UserId == userId && td.TrustedUntil > DateTime.UtcNow)
                .OrderByDescending(td => td.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsDeviceTrustedAsync(int userId, string deviceId)
        {
            return await _context.TrustedDevices
                .AnyAsync(td => td.UserId == userId 
                    && td.DeviceId == deviceId 
                    && td.TrustedUntil > DateTime.UtcNow);
        }

        // Audit log queries
        public async Task<List<AuditLog>> GetAuditLogsAsync(int userId, int pageNumber, int pageSize)
        {
            return await _context.AuditLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
