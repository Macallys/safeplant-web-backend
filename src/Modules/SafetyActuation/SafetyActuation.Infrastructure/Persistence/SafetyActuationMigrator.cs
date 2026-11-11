using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class SafetyActuationMigrator : IHostedService
{
    private readonly IServiceScopeFactory _scopes;

    public SafetyActuationMigrator(IServiceScopeFactory scopes)
    {
        _scopes = scopes;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SafetyActuationDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
