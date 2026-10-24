using Microsoft.EntityFrameworkCore;

namespace PlantMonitoring.Infrastructure.Persistence;

internal static class PlantMonitoringPostgres
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(PlantMonitoringDbContext.MigrationsHistoryTable));
}
