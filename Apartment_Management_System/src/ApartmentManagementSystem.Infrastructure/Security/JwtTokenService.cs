using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagementSystem.Infrastructure.Security
{
    /// <summary>
    /// JWT token service implementation for token generation and validation
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly TokenSettings _tokenSettings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            TokenSettings tokenSettings,
            ApplicationDbContext context,
            ILogger<JwtTokenService> logger)
        {
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a JWT access token with user claims
        /// </summary>
        public string GenerateAccessToken(User user, IEnumerable<string> roles)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Use JWT *short* claim names so the names round-trip through the
                // pipeline unchanged. Pair with MapInboundClaims = false on the
                // JwtBearer side. The validation parameters use NameClaimType="sub"
                // and RoleClaimType="role", so HttpContext.User.FindFirst("sub") /
                // RequireRole(...) just work.
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                    new Claim("userId",                      user.Id.ToString()),
                    new Claim("phone",                       user.Phone ?? string.Empty),
                    new Claim("email_verified",              user.IsEmailVerified ? "true" : "false")
                };

                // Add role claims under the short "role" name.
                foreach (var role in roles)
                {
                    claims.Add(new Claim("role", role));
                }

                var token = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_tokenSettings.ExpirationMinutes),
                    signingCredentials: credentials);

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Generates a secure refresh token and saves to database
        /// </summary>
        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            try
            {
                var randomNumber = new byte[64];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }

                var token = Convert.ToBase64String(randomNumber);
                var tokenHash = ComputeSha256Hash(token);

                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    TokenHash = tokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenExpirationDays),
                    CreatedAt = DateTime.UtcNow,
                    Revoked = false
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token generated for user {UserId}", userId);

                // Return token with hash (token is sent to client, hash is stored in DB)
                refreshToken.TokenHash = token; // Temporarily return plaintext for response
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Validates JWT token and returns claims principal
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _tokenSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _tokenSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid token validation attempt");
                return null;
            }
        }

        /// <summary>
        /// Validates refresh token against database
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(string token, int userId)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var tokenHash = ComputeSha256Hash(token);

                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => 
                        rt.UserId == userId && 
                        rt.TokenHash == tokenHash &&
                        !rt.Revoked &&
                        rt.ExpiresAt > DateTime.UtcNow);

                return refreshToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            try
            {
                var tokenHash = ComputeSha256Hash(token);

                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

                if (refreshToken != null)
                {
                    refreshToken.Revoked = true;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Refresh token revoked for user {UserId}", refreshToken.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                throw;
            }
        }

        /// <summary>
        /// Revokes all refresh tokens for a user (logout operation)
        /// </summary>
        public async Task RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.Revoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.Revoked = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Computes SHA256 hash of token for secure storage
        /// </summary>
        private static string ComputeSha256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
