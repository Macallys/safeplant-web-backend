using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace PlantMonitoring.Infrastructure.Persistence;

internal static class TimescaleSchema
{
    public static async Task ApplyAsync(PlantMonitoringDbContext db, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await ExecuteNonQueryAsync(connection, "CREATE EXTENSION IF NOT EXISTS timescaledb", cancellationToken);
        await ExecuteScalarAsync(
            connection,
            "SELECT create_hypertable('public.carbon_dioxide_readings', 'recorded_at', if_not_exists => TRUE, migrate_data => TRUE)",
            cancellationToken);
        await ExecuteScalarAsync(
            connection,
            "SELECT create_hypertable('public.noise_readings', 'recorded_at', if_not_exists => TRUE, migrate_data => TRUE)",
            cancellationToken);
        await ExecuteScalarAsync(
            connection,
            "SELECT create_hypertable('public.presence_changes', 'recorded_at', if_not_exists => TRUE, migrate_data => TRUE)",
            cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteScalarAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteScalarAsync(cancellationToken);
    }
}
