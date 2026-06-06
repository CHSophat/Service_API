using ApartmentManagementSystem.Domain.Entities.Auth;

namespace ApartmentManagementSystem.Infrastructure.Security
{
    /// <summary>
    /// Service for JWT token generation, validation, and refresh token management
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new JWT access token for the user
        /// </summary>
        /// <param name="user">The user to generate token for</param>
        /// <param name="roles">User roles</param>
        /// <returns>Generated JWT token</returns>
        string GenerateAccessToken(User user, IEnumerable<string> roles);

        /// <summary>
        /// Generates a new refresh token and saves it to database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Generated refresh token</returns>
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId);

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <returns>Claims principal if valid, null otherwise</returns>
        System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Validates a refresh token
        /// </summary>
        /// <param name="token">Refresh token to validate</param>
        /// <param name="userId">Expected user ID</param>
        /// <returns>True if token is valid and not revoked</returns>
        Task<bool> ValidateRefreshTokenAsync(string token, int userId);

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="token">Token to revoke</param>
        Task RevokeRefreshTokenAsync(string token);

        /// <summary>
        /// Revokes all refresh tokens for a user (logout)
        /// </summary>
        /// <param name="userId">User ID</param>
        Task RevokeAllUserTokensAsync(int userId);
    }
}
