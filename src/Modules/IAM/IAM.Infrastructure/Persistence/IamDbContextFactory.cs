using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IAM.Infrastructure.Persistence;

public sealed class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        LoadEnvFile();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings__Postgres is required.");
        }

        var options = new DbContextOptionsBuilder<IamDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IamDbContext(options);
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
