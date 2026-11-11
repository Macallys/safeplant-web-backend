using Microsoft.EntityFrameworkCore;

namespace DeviceEdgeManagement.Infrastructure.Persistence;

internal static class DeviceEdgePostgres
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(DeviceEdgeDbContext.MigrationsHistoryTable));
}
