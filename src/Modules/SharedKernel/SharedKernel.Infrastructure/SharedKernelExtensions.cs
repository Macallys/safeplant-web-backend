using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain;

namespace SharedKernel.Infrastructure;

public static class SharedKernelExtensions
{
    public static IServiceCollection AddSystemClock(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }

    public static IApplicationBuilder UseApiErrors(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiErrorMiddleware>();
    }
}
