using Microsoft.Extensions.DependencyInjection;

namespace IAM.Infrastructure;

public static class IamServiceCollectionExtensions
{
    public static IServiceCollection AddIam(this IServiceCollection services)
    {
        return services;
    }
}
