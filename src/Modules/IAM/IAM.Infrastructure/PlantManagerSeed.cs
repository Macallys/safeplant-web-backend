using IAM.Application;
using IAM.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IAM.Infrastructure;

public sealed class PlantManagerSeed : IHostedService
{
    private readonly IServiceScopeFactory _scopes;

    public PlantManagerSeed(IServiceScopeFactory scopes)
    {
        _scopes = scopes;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<IamSettings>>().Value;
        if (string.IsNullOrWhiteSpace(settings.Seed.Email) || string.IsNullOrWhiteSpace(settings.Seed.Password))
        {
            throw new InvalidOperationException("Iam:Seed:Email and Iam:Seed:Password are required to create the first account.");
        }

        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Accounts.Add(new UserAccount(
            Guid.NewGuid(),
            settings.Seed.Email,
            passwords.Hash(settings.Seed.Password),
            Role.PlantManager,
            enabled: true));

        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
