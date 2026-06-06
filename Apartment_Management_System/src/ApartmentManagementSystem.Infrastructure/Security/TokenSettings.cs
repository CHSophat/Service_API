namespace ApartmentManagementSystem.Infrastructure.Security
{
    /// <summary>
    /// Configuration settings for JWT token generation
    /// </summary>
    public class TokenSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
