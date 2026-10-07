using Microsoft.Extensions.DependencyInjection;

namespace SafetyActuation.Infrastructure;

public static class SafetyActuationServiceCollectionExtensions
{
    public static IServiceCollection AddSafetyActuation(this IServiceCollection services)
    {
        return services;
    }
}
