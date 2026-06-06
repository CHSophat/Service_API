namespace ApartmentManagementSystem.Infrastructure.Security;

/// <summary>
/// Authentication contract used by API endpoints. Encapsulates the JWT signing
/// key, password verification, refresh-token lifecycle, and the
/// Authorize/Unauthorize decisions (login = Authorize, logout = Unauthorize).
/// Lives in Infrastructure because the project layering in this repo is
/// Application → Infrastructure (not the typical inverted form).
/// </summary>
public interface IAuthService
{
    /// <summary>Email + password sign-in (Authorize). Returns tokens and roles on success.</summary>
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Rotate access token using a valid refresh token.</summary>
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Sign-out (Unauthorize). If <paramref name="allDevices"/>, every refresh token is revoked.</summary>
    Task LogoutAsync(int userId, string? refreshToken, bool allDevices, CancellationToken ct = default);

    /// <summary>Returns true if the access token's signature, issuer, audience, and lifetime are all valid.</summary>
    bool ValidateAccessToken(string accessToken);
}

/// <summary>Outcome of a Login or Refresh attempt.</summary>
public sealed class AuthResult
{
    public bool Succeeded { get; init; }
    public AuthFailureReason? Reason { get; init; }
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresUtc { get; init; }
    public bool Is2FaRequired { get; init; }
    public bool IsEmailVerified { get; init; }

    public static AuthResult Fail(AuthFailureReason reason) => new() { Succeeded = false, Reason = reason };
}

public enum AuthFailureReason
{
    InvalidCredentials,
    AccountDisabled,
    AccountLocked,
    InvalidRefreshToken,
    UserNotFound
}
