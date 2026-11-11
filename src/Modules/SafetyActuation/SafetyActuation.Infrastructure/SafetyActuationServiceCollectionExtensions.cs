using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Infrastructure.Persistence;

namespace SafetyActuation.Infrastructure;

public static class SafetyActuationServiceCollectionExtensions
{
    public static IServiceCollection AddSafetyActuation(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddSafetyActuation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        }

        services.AddDbContext<SafetyActuationDbContext>(options =>
            SafetyActuationPostgres.Configure(options, connectionString));
        services.AddScoped<IAreaSafetyStatus, EfAreaSafetyStatus>();
        services.AddHostedService<SafetyActuationMigrator>();
        return services;
    }
}
