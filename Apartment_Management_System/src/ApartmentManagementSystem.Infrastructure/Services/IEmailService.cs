namespace ApartmentManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Email service for sending transactional emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email message
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlContent">HTML body content</param>
        /// <param name="plainTextContent">Plain text body content (optional)</param>
        /// <returns>Success indicator</returns>
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null);

        /// <summary>
        /// Sends email verification link
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="verificationToken">Verification token</param>
        /// <param name="verificationUrl">Base URL for verification</param>
        Task<bool> SendEmailVerificationAsync(string email, string verificationToken, string verificationUrl);

        /// <summary>
        /// Sends password reset link
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="resetToken">Password reset token</param>
        /// <param name="resetUrl">Base URL for password reset</param>
        Task<bool> SendPasswordResetAsync(string email, string resetToken, string resetUrl);

        /// <summary>
        /// Sends OTP code via email
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="otpCode">One-time password</param>
        Task<bool> SendOtpAsync(string email, string otpCode);

        /// <summary>
        /// Sends 2FA authentication code
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="code">Authentication code</param>
        Task<bool> Send2FaCodeAsync(string email, string code);

        /// <summary>
        /// Sends welcome email to new user
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="userName">User name</param>
        Task<bool> SendWelcomeEmailAsync(string email, string userName);
    }
}
