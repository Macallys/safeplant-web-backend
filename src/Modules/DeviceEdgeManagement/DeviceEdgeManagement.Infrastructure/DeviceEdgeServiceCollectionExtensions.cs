using DeviceEdgeManagement.Application.Services;
using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using DeviceEdgeManagement.Infrastructure.Persistence;
using DeviceEdgeManagement.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Infrastructure;

public static class DeviceEdgeServiceCollectionExtensions
{
    public static IServiceCollection AddDeviceEdge(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddDeviceEdge(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        }

        var settings = ReadSettings(configuration.GetSection("DeviceEdge"));
        EnsureSettings(settings);
        services.Configure<DeviceEdgeSettings>(configuration.GetSection("DeviceEdge"));

        services.AddDbContext<DeviceEdgeDbContext>(options =>
            DeviceEdgePostgres.Configure(options, connectionString));
        services.AddSingleton<ISecretHasher, Sha256SecretHasher>();
        services.AddScoped<IDeviceCredentialStore, EfDeviceCredentialStore>();
        services.AddScoped<IDeviceCredentialService>(serviceProvider => new DeviceCredentialService(
            serviceProvider.GetRequiredService<IDeviceCredentialStore>(),
            serviceProvider.GetRequiredService<IAreaStore>(),
            serviceProvider.GetRequiredService<IDeviceStore>(),
            serviceProvider.GetRequiredService<IDeviceService>(),
            serviceProvider.GetRequiredService<ISecretHasher>(),
            serviceProvider.GetRequiredService<IClock>(),
            settings.CredentialDays));
        services.AddHostedService<DeviceEdgeMigrator>();
        return services;
    }

    private static DeviceEdgeSettings ReadSettings(IConfigurationSection section)
    {
        var settings = new DeviceEdgeSettings();
        settings.CredentialDays = ReadInt(section, nameof(DeviceEdgeSettings.CredentialDays), settings.CredentialDays);
        return settings;
    }

    private static int ReadInt(IConfigurationSection section, string key, int fallback)
    {
        var raw = section[key];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        if (!int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidOperationException($"DeviceEdge:{key} must be an integer.");
        }

        return value;
    }

    private static void EnsureSettings(DeviceEdgeSettings settings)
    {
        if (settings.CredentialDays < 1)
        {
            throw new InvalidOperationException("DeviceEdge:CredentialDays must be greater than zero.");
        }
    }
}
