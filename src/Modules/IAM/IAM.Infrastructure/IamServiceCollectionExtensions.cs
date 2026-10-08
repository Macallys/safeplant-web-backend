using System.Text;
using IAM.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Domain;

namespace IAM.Infrastructure;

public static class IamServiceCollectionExtensions
{
    public static IServiceCollection AddIam(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        }

        services.Configure<IamSettings>(configuration.GetSection("Iam"));

        services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ISecretHasher, Sha256SecretHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IAccessTokenReader, JwtAccessTokenReader>();
        services.AddScoped<IIamStore, EfIamStore>();
        services.AddScoped(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<IamSettings>>().Value;
            EnsureSettings(settings);

            return new SignInService(
                serviceProvider.GetRequiredService<IIamStore>(),
                serviceProvider.GetRequiredService<IPasswordHasher>(),
                serviceProvider.GetRequiredService<IAccessTokenIssuer>(),
                serviceProvider.GetRequiredService<IClock>(),
                TimeSpan.FromHours(settings.SessionHours));
        });
        services.AddScoped(serviceProvider => new SessionAccess(
            serviceProvider.GetRequiredService<IIamStore>(),
            serviceProvider.GetRequiredService<IAccessTokenReader>(),
            serviceProvider.GetRequiredService<IClock>()));
        services.AddScoped(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<IamSettings>>().Value;
            EnsureSettings(settings);

            return new RecoveryService(
                serviceProvider.GetRequiredService<IIamStore>(),
                serviceProvider.GetRequiredService<IPasswordHasher>(),
                serviceProvider.GetRequiredService<ISecretHasher>(),
                serviceProvider.GetRequiredService<IClock>(),
                TimeSpan.FromHours(settings.RecoveryHours),
                serviceProvider.GetRequiredService<ILogger<RecoveryService>>());
        });
        services.AddScoped(serviceProvider => new DirectoryService(
            serviceProvider.GetRequiredService<IIamStore>(),
            serviceProvider.GetRequiredService<IPasswordHasher>()));
        services.AddHostedService<PlantManagerSeed>();

        return services;
    }

    private static void EnsureSettings(IamSettings settings)
    {
        if (settings.SessionHours <= 0)
        {
            throw new InvalidOperationException("Iam:SessionHours must be greater than zero.");
        }

        if (settings.RecoveryHours <= 0)
        {
            throw new InvalidOperationException("Iam:RecoveryHours must be greater than zero.");
        }

        if (Encoding.UTF8.GetByteCount(settings.SigningKey) < 32)
        {
            throw new InvalidOperationException("Iam:SigningKey must be at least 32 characters.");
        }
    }
}
