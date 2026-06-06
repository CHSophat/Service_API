using System;
using System.Collections.Generic;

namespace ApartmentManagementSystem.Application.DTOs.AuthDto
{
    // Request DTOs
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
        public string? CustomerType { get; set; } // For linking to customer module if needed
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class OtpRequestDto
    {
        public required string Phone { get; set; }
        public string Purpose { get; set; } = "login"; // login, password_reset
    }

    public class OtpVerifyRequest
    {
        public required string Phone { get; set; }
        public required string OtpCode { get; set; }
        public bool RememberDevice { get; set; }
        public required string DeviceId { get; set; }
        public required string DeviceName { get; set; }
    }

    public class SsoLoginRequest
    {
        public required string IdToken { get; set; } // From Google/Apple
        public required string Provider { get; set; } // "google" or "apple"
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class Enable2FaRequest
    {
        public required string Password { get; set; }
    }

    public class Verify2FaRequest
    {
        public required string TotpCode { get; set; }
        public required string[] BackupCodes { get; set; }
    }

    public class Submit2FaCodeRequest
    {
        public required string TotpCode { get; set; }
        public required string UserId { get; set; }
        public bool RememberDevice { get; set; }
        public required string DeviceId { get; set; }
    }

    public class LogoutRequest
    {
        public required string RefreshToken { get; set; }
        public bool LogoutAllDevices { get; set; }
    }

    public class VerifyEmailRequest
    {
        public required string Email { get; set; }
        public required string VerificationCode { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<string> Roles { get; set; } = new();
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; } = 3600; // 1 hour in seconds
        public DateTime AccessTokenExpires { get; set; }
        public bool Is2FaRequired { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class RegisterResponse
    {
        public bool Success { get; set; } = false;
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Is2FaEnabled { get; set; }
        public required string[] Roles { get; set; }
        /// <summary>
        /// Flattened permission codes ("resource:action") computed from the
        /// user's roles via the server-side matrix
        /// (API.Endpoints.Permissions.For). The web client uses this to gate
        /// UI; the actual enforcement still lives in the endpoint authz.
        /// </summary>
        public string[] Permissions { get; set; } = Array.Empty<string>();
    }

    public class SessionResponse
    {
        public int SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ActiveSessionsResponse
    {
        public required List<SessionResponse> Sessions { get; set; }
        public int TotalActiveSessions { get; set; }
    }

    public class OtpResponseDto
    {
        public int OtpId { get; set; }
        public string? Phone { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Message { get; set; }
    }

    public class Enable2FaResponse
    {
        public required string TotpSecret { get; set; }
        public required string QrCodeUrl { get; set; }
        public required string[] BackupCodes { get; set; }
        public required string Message { get; set; }
    }

    public class TokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // DTO for internal use
    public class UserAuthDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool Is2FaEnabled { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
