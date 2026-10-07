using Microsoft.Extensions.DependencyInjection;

namespace PlantMonitoring.Infrastructure;

public static class PlantMonitoringServiceCollectionExtensions
{
    public static IServiceCollection AddPlantMonitoring(this IServiceCollection services)
    {
        return services;
    }
}
