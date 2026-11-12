using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class PlantMonitoringMigrator : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _configuration;

    public PlantMonitoringMigrator(IServiceScopeFactory scopes, IConfiguration configuration)
    {
        _scopes = scopes;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PlantMonitoringDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        if (ReadEnabled(_configuration))
        {
            await TimescaleSchema.ApplyAsync(db, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool ReadEnabled(IConfiguration configuration)
    {
        var raw = configuration["Timescale:Enabled"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!bool.TryParse(raw, out var enabled))
        {
            throw new InvalidOperationException("Timescale:Enabled must be true or false.");
        }

        return enabled;
    }
}
