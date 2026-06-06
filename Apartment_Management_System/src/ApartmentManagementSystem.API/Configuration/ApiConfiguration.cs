namespace ApartmentManagementSystem.API.Configuration
{
    /// <summary>
    /// Centralized API configuration for all environments
    /// </summary>
    public class ApiConfiguration
    {
        public string Version { get; set; } = "v1";
        public string Environment { get; set; } = "Development";
        public string ApiUrl { get; set; } = "";
        public string WebUrl { get; set; } = "";
        public string MobileUrl { get; set; } = "";
    }

    /// <summary>
    /// CORS configuration settings
    /// </summary>
    public class CorsConfiguration
    {
        public List<string> AllowedOrigins { get; set; } = new();
    }

    /// <summary>
    /// JWT authentication configuration
    /// </summary>
    public class JwtConfiguration
    {
        public string Secret { get; set; } = "";
        public int ExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public class RateLimitingConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int RequestsPerMinute { get; set; } = 100;
    }

    /// <summary>
    /// Email service configuration
    /// </summary>
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public bool IsProduction { get; set; } = false;
    }

    /// <summary>
    /// Payment service configuration (Stripe)
    /// </summary>
    public class PaymentConfiguration
    {
        public string PublishableKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
    }

    /// <summary>
    /// Firebase configuration
    /// </summary>
    public class FirebaseConfiguration
    {
        public string ProjectId { get; set; } = "";
        public string PrivateKeyId { get; set; } = "";
        public string PrivateKey { get; set; } = "";
    }
}
