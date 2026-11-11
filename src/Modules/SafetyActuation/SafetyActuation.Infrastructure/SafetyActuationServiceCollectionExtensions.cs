using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Application.Services;
using SafetyActuation.Domain.Interfaces;
using SafetyActuation.Domain.Services;
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
        services.AddScoped<IActuatorStore, EfActuatorStore>();
        services.AddScoped<ISafetyRecordStore, EfSafetyRecordStore>();
        services.AddScoped<IOperationalStatusService, OperationalStatusService>();
        services.AddScoped<ISafetyReadService, SafetyReadService>();
        services.AddScoped<IOverrideService, OverrideService>();
        services.AddHostedService<SafetyActuationMigrator>();
        return services;
    }
}
