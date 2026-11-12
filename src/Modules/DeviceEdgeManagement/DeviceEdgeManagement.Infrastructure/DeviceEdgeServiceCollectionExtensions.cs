using DeviceEdgeManagement.Application.Services;
using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using DeviceEdgeManagement.Infrastructure.Mqtt;
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
        var plantKey = configuration["Ingest:PlantKey"] ?? "";
        services.AddScoped<ITelemetryIngestService>(serviceProvider => new TelemetryIngestService(
            plantKey,
            serviceProvider.GetRequiredService<IDeviceCredentialStore>(),
            serviceProvider.GetRequiredService<IDeviceStore>(),
            serviceProvider.GetRequiredService<IReadingStore>(),
            serviceProvider.GetRequiredService<ISecretHasher>(),
            serviceProvider.GetRequiredService<IClock>()));
        return services;
    }

    public static IServiceCollection AddDeviceEdgePlant(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = ReadPlantSettings(configuration);
        services.AddSingleton(settings);
        services.AddHttpClient(MqttTelemetryWorker.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHostedService<MqttTelemetryWorker>();
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

    private static PlantEdgeSettings ReadPlantSettings(IConfiguration configuration)
    {
        var host = configuration["Mqtt:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Mqtt:Host is required.");
        }

        var port = ReadPort(configuration["Mqtt:Port"]);
        var ingestUrl = configuration["Cloud:IngestUrl"];
        if (!Uri.TryCreate(ingestUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Cloud:IngestUrl must be an absolute http or https URL.");
        }

        var plantKey = configuration["Ingest:PlantKey"];
        if (string.IsNullOrWhiteSpace(plantKey))
        {
            throw new InvalidOperationException("Ingest:PlantKey is required.");
        }

        return new PlantEdgeSettings(host.Trim(), port, uri, plantKey);
    }

    private static int ReadPort(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 1883;
        }

        if (!int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var port)
            || port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Mqtt:Port must be an integer from 1 to 65535.");
        }

        return port;
    }
}
