using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class SafetyActuationDbContextFactory : IDesignTimeDbContextFactory<SafetyActuationDbContext>
{
    public SafetyActuationDbContext CreateDbContext(string[] args)
    {
        LoadEnvFile();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings__Postgres is required.");
        }

        var options = new DbContextOptionsBuilder<SafetyActuationDbContext>();
        SafetyActuationPostgres.Configure(options, connectionString);
        return new SafetyActuationDbContext(options.Options);
    }

    private static void LoadEnvFile()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (File.Exists(path))
            {
                DotNetEnv.Env.NoClobber().Load(path);
                return;
            }

            directory = directory.Parent;
        }
    }
}
