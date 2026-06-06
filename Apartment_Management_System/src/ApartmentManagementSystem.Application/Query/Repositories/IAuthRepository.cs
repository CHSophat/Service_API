using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Application.Query.Repositories
{
    public interface IAuthRepository
    {
        // User queries
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByPhoneAsync(string phone);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByPhoneAsync(string phone);

        // Role queries
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<List<Role>> GetUserRolesAsync(int userId);
        Task<bool> UserHasRoleAsync(int userId, string roleName);

        // Refresh token queries
        Task<RefreshToken> GetRefreshTokenByIdAsync(int tokenId);
        Task<RefreshToken> GetRefreshTokenByHashAsync(string tokenHash);
        Task<List<RefreshToken>> GetActiveSessionsAsync(int userId);
        Task<int> CountActiveSessionsAsync(int userId);
        Task<bool> IsTokenRevokedAsync(string tokenHash);

        // OTP queries
        Task<OtpCode> GetValidOtpAsync(string phone, string code);
        Task<OtpCode> GetOtpByIdAsync(int otpId);
        Task<bool> HasUnusedOtpAsync(string phone);

        // Trusted device queries
        Task<TrustedDevice> GetTrustedDeviceAsync(int userId, string deviceId);
        Task<List<TrustedDevice>> GetTrustedDevicesAsync(int userId);
        Task<bool> IsDeviceTrustedAsync(int userId, string deviceId);

        // Audit log queries
        Task<List<AuditLog>> GetAuditLogsAsync(int userId, int pageNumber, int pageSize);
    }
}
