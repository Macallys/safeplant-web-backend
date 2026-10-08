using System.Text;
using IAM.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<ISignInStore, EfSignInStore>();
        services.AddScoped(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<IamSettings>>().Value;
            if (settings.SessionHours <= 0)
            {
                throw new InvalidOperationException("Iam:SessionHours must be greater than zero.");
            }

            if (Encoding.UTF8.GetByteCount(settings.SigningKey) < 32)
            {
                throw new InvalidOperationException("Iam:SigningKey must be at least 32 characters.");
            }

            return new SignInService(
                serviceProvider.GetRequiredService<ISignInStore>(),
                serviceProvider.GetRequiredService<IPasswordHasher>(),
                serviceProvider.GetRequiredService<IAccessTokenIssuer>(),
                serviceProvider.GetRequiredService<IClock>(),
                TimeSpan.FromHours(settings.SessionHours));
        });
        services.AddHostedService<PlantManagerSeed>();

        return services;
    }
}
