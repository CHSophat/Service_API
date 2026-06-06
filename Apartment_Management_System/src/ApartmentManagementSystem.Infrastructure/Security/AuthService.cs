using ApartmentManagementSystem.Domain.Entities.Auth;
using ApartmentManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApartmentManagementSystem.Infrastructure.Security;

/// <summary>
/// Concrete <see cref="IAuthService"/>. Combines password verification, JWT
/// signing (via <see cref="ITokenService"/>), and refresh-token lifecycle.
/// Endpoints should depend on <c>IAuthService</c>, not on the lower-level
/// services directly.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly ITokenService _tokens;
    private readonly TokenSettings _tokenSettings;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenService tokens,
        TokenSettings tokenSettings,
        ApplicationDbContext db,
        ILogger<AuthService> logger)
    {
        _tokens = tokens;
        _tokenSettings = tokenSettings;
        _db = db;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
            return AuthResult.Fail(AuthFailureReason.InvalidCredentials);

        if (!user.IsActive)
            return AuthResult.Fail(AuthFailureReason.AccountDisabled);

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            return AuthResult.Fail(AuthFailureReason.AccountLocked);

        var passwordOk = !string.IsNullOrEmpty(user.PasswordHash)
                         && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!passwordOk)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync(ct);
            return AuthResult.Fail(AuthFailureReason.InvalidCredentials);
        }

        var roles = await LoadRolesAsync(user.Id, ct);
        var access = _tokens.GenerateAccessToken(user, roles);
        var refresh = await _tokens.GenerateRefreshTokenAsync(user.Id);

        user.LastLoginAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} signed in", user.Id);

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Roles = roles,
            AccessToken = access,
            RefreshToken = refresh.TokenHash,
            AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(_tokenSettings.ExpirationMinutes),
            Is2FaRequired = false,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthResult.Fail(AuthFailureReason.InvalidRefreshToken);

        var hash = ComputeSha256(refreshToken);
        var stored = await _db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash && !rt.Revoked && rt.ExpiresAt > DateTime.UtcNow, ct);

        if (stored is null)
            return AuthResult.Fail(AuthFailureReason.InvalidRefreshToken);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, ct);
        if (user is null || !user.IsActive)
            return AuthResult.Fail(AuthFailureReason.UserNotFound);

        var roles = await LoadRolesAsync(user.Id, ct);
        var access = _tokens.GenerateAccessToken(user, roles);
        // Rotate: revoke old + issue new
        await _tokens.RevokeRefreshTokenAsync(refreshToken);
        var newRefresh = await _tokens.GenerateRefreshTokenAsync(user.Id);

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Roles = roles,
            AccessToken = access,
            RefreshToken = newRefresh.TokenHash,
            AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(_tokenSettings.ExpirationMinutes),
            Is2FaRequired = false,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    public async Task LogoutAsync(int userId, string? refreshToken, bool allDevices, CancellationToken ct = default)
    {
        if (allDevices)
        {
            await _tokens.RevokeAllUserTokensAsync(userId);
            _logger.LogInformation("User {UserId} signed out from all devices", userId);
            return;
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _tokens.RevokeRefreshTokenAsync(refreshToken);
        }
        _logger.LogInformation("User {UserId} signed out", userId);
    }

    public bool ValidateAccessToken(string accessToken) =>
        !string.IsNullOrEmpty(accessToken) && _tokens.ValidateToken(accessToken) is not null;

    private async Task<List<string>> LoadRolesAsync(int userId, CancellationToken ct)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync(ct);
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hashed = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashed);
    }
}
