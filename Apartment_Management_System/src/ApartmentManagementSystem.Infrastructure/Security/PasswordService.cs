using BC = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Logging;

namespace ApartmentManagementSystem.Infrastructure.Security
{
    /// <summary>
    /// Service for password hashing and verification using BCrypt
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a password against its hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Password hash</param>
        /// <returns>True if password matches hash</returns>
        bool VerifyPassword(string password, string hash);
    }

    /// <summary>
    /// BCrypt-based password service implementation
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            try
            {
                return BC.HashPassword(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BC.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying password");
                return false;
            }
        }
    }
}
