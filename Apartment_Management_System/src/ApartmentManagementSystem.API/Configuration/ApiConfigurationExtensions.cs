namespace ApartmentManagementSystem.API.Configuration
{
    /// <summary>
    /// Extension methods for registering API configuration services
    /// </summary>
    public static class ApiConfigurationExtensions
    {
        /// <summary>
        /// Adds API configuration services to the DI container
        /// </summary>
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration objects
            services.Configure<ApiConfiguration>(configuration.GetSection("ApiConfiguration"));
            services.Configure<CorsConfiguration>(configuration.GetSection("Cors"));
            services.Configure<JwtConfiguration>(configuration.GetSection("Jwt"));
            services.Configure<RateLimitingConfiguration>(configuration.GetSection("RateLimiting"));
            services.Configure<EmailConfiguration>(configuration.GetSection("Email"));
            services.Configure<PaymentConfiguration>(configuration.GetSection("Stripe"));
            services.Configure<FirebaseConfiguration>(configuration.GetSection("Firebase"));

            return services;
        }

        /// <summary>
        /// Adds environment-specific CORS policies
        /// </summary>
        public static IServiceCollection AddEnvironmentSpecificCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsConfig = configuration.GetSection("Cors").Get<CorsConfiguration>();
            
            services.AddCors(options =>
            {
                // API endpoints policy
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy
                        .WithOrigins(corsConfig?.AllowedOrigins?.ToArray() ?? new[] { "http://localhost:3000" })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

                // Development policy (more permissive)
                options.AddPolicy("DevelopmentPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }

        /// <summary>
        /// Gets the appropriate CORS policy based on environment
        /// </summary>
        public static string GetCorsPolicyName(string environment)
        {
            return environment switch
            {
                "Development" => "DevelopmentPolicy",
                _ => "ApiPolicy"
            };
        }
    }
}
