using ApartmentManagementSystem.Infrastructure.Persistence;
using ApartmentManagementSystem.Infrastructure.Security;
using ApartmentManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmtpSettings = ApartmentManagementSystem.Infrastructure.Services.SmtpSettings;

namespace ApartmentManagementSystem.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString,
        TokenSettings tokenSettings,
        SmtpSettings smtpSettings
        )
    {
        // Register DbContext with PostgreSQL.
        //
        // UseSnakeCaseNamingConvention() makes EF translate PascalCase CLR
        // names (Id, FirstName, ProductType, CustomerId, ...) to snake_case
        // column / index / FK names (id, first_name, product_type, ...).
        // The actual Postgres schema is snake_case; without this, every query
        // generated SQL like c."Id" and failed with 42703 column-not-found.
        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null))
                .UseSnakeCaseNamingConvention());

        // Register Security Services
        services.AddSingleton(tokenSettings);
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register Email Service
        services.AddSingleton(smtpSettings);
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
