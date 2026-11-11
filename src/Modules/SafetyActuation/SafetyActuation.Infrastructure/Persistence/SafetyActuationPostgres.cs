using Microsoft.EntityFrameworkCore;

namespace SafetyActuation.Infrastructure.Persistence;

internal static class SafetyActuationPostgres
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(SafetyActuationDbContext.MigrationsHistoryTable));
}
