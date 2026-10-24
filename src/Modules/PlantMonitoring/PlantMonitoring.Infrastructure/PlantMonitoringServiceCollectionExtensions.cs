using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantMonitoring.Application.Services;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using PlantMonitoring.Infrastructure.Persistence;

namespace PlantMonitoring.Infrastructure;

public static class PlantMonitoringServiceCollectionExtensions
{
    public static IServiceCollection AddPlantMonitoring(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        }

        var settings = ReadSettings(configuration.GetSection("PlantMonitoring"));
        EnsureSettings(settings);
        services.Configure<PlantMonitoringSettings>(configuration.GetSection("PlantMonitoring"));

        services.AddDbContext<PlantMonitoringDbContext>(options =>
            PlantMonitoringPostgres.Configure(options, connectionString));
        services.AddScoped<IAreaStore, EfAreaStore>();
        services.AddScoped<IAreaService, AreaService>();
        services.AddHostedService<PlantMonitoringMigrator>();

        return services;
    }

    private static PlantMonitoringSettings ReadSettings(IConfigurationSection section)
    {
        var settings = new PlantMonitoringSettings();
        settings.Co2MinPpm = ReadInt(section, nameof(PlantMonitoringSettings.Co2MinPpm), settings.Co2MinPpm);
        settings.Co2MaxPpm = ReadInt(section, nameof(PlantMonitoringSettings.Co2MaxPpm), settings.Co2MaxPpm);
        settings.NoiseMinDb = ReadInt(section, nameof(PlantMonitoringSettings.NoiseMinDb), settings.NoiseMinDb);
        settings.NoiseMaxDb = ReadInt(section, nameof(PlantMonitoringSettings.NoiseMaxDb), settings.NoiseMaxDb);
        settings.HistoryMaxDays = ReadInt(section, nameof(PlantMonitoringSettings.HistoryMaxDays), settings.HistoryMaxDays);
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
            throw new InvalidOperationException($"PlantMonitoring:{key} must be an integer.");
        }

        return value;
    }

    private static void EnsureSettings(PlantMonitoringSettings settings)
    {
        if (settings.Co2MinPpm < 1 || settings.Co2MaxPpm < settings.Co2MinPpm)
        {
            throw new InvalidOperationException("PlantMonitoring CO2 bounds are invalid.");
        }

        if (settings.NoiseMinDb < 1 || settings.NoiseMaxDb < settings.NoiseMinDb)
        {
            throw new InvalidOperationException("PlantMonitoring noise bounds are invalid.");
        }

        if (settings.HistoryMaxDays < 1)
        {
            throw new InvalidOperationException("PlantMonitoring:HistoryMaxDays must be greater than zero.");
        }
    }
}
