using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApartmentManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Configuration for SMTP email service
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = default!;
        public bool EnableSsl { get; set; } = true;
    }

    /// <summary>
    /// SMTP-based email service implementation
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpSettings> smtpSettings, ILogger<SmtpEmailService> logger)
        {
            _smtpSettings = smtpSettings?.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null)
        {
            if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject))
            {
                _logger.LogWarning("Invalid email parameters: to={To}, subject={Subject}", to, subject);
                return false;
            }

            try
            {
                using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
                {
                    client.EnableSsl = _smtpSettings.EnableSsl;
                    client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                        Subject = subject,
                        Body = htmlContent ?? plainTextContent,
                        IsBodyHtml = !string.IsNullOrEmpty(htmlContent)
                    };

                    mailMessage.To.Add(to);

                    // Add plain text alternative if HTML is provided
                    if (!string.IsNullOrEmpty(htmlContent) && !string.IsNullOrEmpty(plainTextContent))
                    {
                        var plainView = AlternateView.CreateAlternateViewFromString(plainTextContent, null, "text/plain");
                        var htmlView = AlternateView.CreateAlternateViewFromString(htmlContent, null, "text/html");
                        mailMessage.AlternateViews.Add(plainView);
                        mailMessage.AlternateViews.Add(htmlView);
                    }

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {To}", to);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string verificationToken, string verificationUrl)
        {
            var verifyLink = $"{verificationUrl}?token={Uri.EscapeDataString(verificationToken)}";
            var subject = "Verify Your Email - Apartment Management System";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Email Verification</h2>
                        <p>Thank you for registering! Please verify your email address by clicking the link below:</p>
                        <p><a href='{verifyLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Verify Email</a></p>
                        <p>Or copy this link: {verifyLink}</p>
                        <p>This link expires in 24 hours.</p>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlContent, $"Click here to verify: {verifyLink}");
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetToken, string resetUrl)
        {
            var resetLink = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}";
            var subject = "Reset Your Password - Apartment Management System";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Password Reset Request</h2>
                        <p>We received a request to reset your password. Click the link below to set a new password:</p>
                        <p><a href='{resetLink}' style='background-color: #008CBA; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Reset Password</a></p>
                        <p>Or copy this link: {resetLink}</p>
                        <p>This link expires in 1 hour. If you didn't request this, please ignore this email.</p>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlContent, $"Click here to reset password: {resetLink}");
        }

        public async Task<bool> SendOtpAsync(string email, string otpCode)
        {
            var subject = "Your One-Time Password - Apartment Management System";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>One-Time Password (OTP)</h2>
                        <p>Your one-time password is:</p>
                        <h1 style='text-align: center; color: #4CAF50; font-size: 36px; letter-spacing: 2px;'>{otpCode}</h1>
                        <p>This code expires in 10 minutes.</p>
                        <p><strong>Do not share this code with anyone.</strong></p>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlContent, $"Your OTP: {otpCode}");
        }

        public async Task<bool> Send2FaCodeAsync(string email, string code)
        {
            var subject = "Your Two-Factor Authentication Code - Apartment Management System";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Two-Factor Authentication Code</h2>
                        <p>Your authentication code is:</p>
                        <h1 style='text-align: center; color: #FF9800; font-size: 36px; letter-spacing: 2px;'>{code}</h1>
                        <p>This code expires in 5 minutes.</p>
                        <p><strong>Do not share this code with anyone.</strong></p>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlContent, $"Your 2FA code: {code}");
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = "Welcome to Apartment Management System!";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Welcome, {userName}!</h2>
                        <p>Thank you for joining our Apartment Management System.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Manage your apartment information</li>
                            <li>Track maintenance requests</li>
                            <li>Communicate with property managers</li>
                            <li>View lease documents</li>
                        </ul>
                        <p>If you have any questions, please contact our support team.</p>
                        <p>Happy managing!</p>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlContent);
        }
    }
}
