using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DeviceEdgeManagement.Infrastructure.Persistence;

public sealed class SqlitePlantLocalStore : IPlantLocalStore
{
    private const string TelemetryKind = "telemetry";
    private const string SafetyKind = "safety";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly DbContextOptions<PlantLocalDbContext> _options;
    private readonly string? _directory;
    private bool _ready;

    public SqlitePlantLocalStore(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        _directory = DirectoryOf(builder.DataSource);
        _options = new DbContextOptionsBuilder<PlantLocalDbContext>().UseSqlite(builder.ConnectionString).Options;
    }

    public Task ReplaceSheetAsync(PlantSheet sheet, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await db.Areas.ExecuteDeleteAsync(cancellationToken);
            await db.Thresholds.ExecuteDeleteAsync(cancellationToken);
            await db.Devices.ExecuteDeleteAsync(cancellationToken);
            await db.Credentials.ExecuteDeleteAsync(cancellationToken);
            foreach (var area in sheet.Areas)
            {
                db.Areas.Add(new PlantAreaRow
                {
                    AreaId = area.Id,
                    Name = area.Name,
                    Location = area.Location,
                    UpdatedAt = area.UpdatedAt,
                });
            }

            foreach (var threshold in sheet.Thresholds)
            {
                db.Thresholds.Add(new PlantThresholdRow
                {
                    AreaId = threshold.AreaId,
                    Co2LimitPpm = threshold.Co2LimitPpm,
                    NoiseLimitDb = threshold.NoiseLimitDb,
                    UpdatedAt = threshold.UpdatedAt,
                });
            }

            foreach (var device in sheet.Devices)
            {
                db.Devices.Add(new PlantDeviceRow
                {
                    DeviceId = device.DeviceId,
                    AreaId = device.AreaId,
                    Kind = device.Kind,
                });
            }

            foreach (var credential in sheet.Credentials)
            {
                db.Credentials.Add(new PlantCredentialRow
                {
                    DeviceId = credential.DeviceId,
                    SecretHash = credential.SecretHash,
                    ExpiresAt = credential.ExpiresAt,
                    RevokedAt = credential.RevokedAt,
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }, cancellationToken);

    public Task<PlantCredentialReplica?> FindCredentialAsync(string deviceId, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            var row = await db.Credentials.SingleOrDefaultAsync(entry => entry.DeviceId == deviceId, cancellationToken);
            return row is null
                ? null
                : new PlantCredentialReplica(row.DeviceId, row.SecretHash, row.ExpiresAt, row.RevokedAt);
        }, cancellationToken);

    public Task<PlantDeviceReplica?> FindDeviceAsync(string deviceId, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            var row = await db.Devices.SingleOrDefaultAsync(entry => entry.DeviceId == deviceId, cancellationToken);
            return row is null ? null : new PlantDeviceReplica(row.DeviceId, row.AreaId, row.Kind);
        }, cancellationToken);

    public Task SaveReadingAsync(LocalReading reading, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await UpsertReadingAsync(db, reading, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

    public Task EnqueueTelemetryAsync(LocalReading? reading, string body, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            if (reading is not null)
            {
                await UpsertReadingAsync(db, reading, cancellationToken);
            }

            db.Outbound.Add(new OutboundRow
            {
                Kind = TelemetryKind,
                Body = body,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }, cancellationToken);

    public Task<OutboundMessage?> PeekTelemetryAsync(CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            var row = await db.Outbound
                .Where(entry => entry.Kind == TelemetryKind)
                .OrderBy(entry => entry.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return row is null ? null : new OutboundMessage(row.Id, row.Body);
        }, cancellationToken);

    public Task DropAsync(long id, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await db.Outbound.Where(entry => entry.Id == id).ExecuteDeleteAsync(cancellationToken);
        }, cancellationToken);

    public Task<IReadOnlyList<AreaLoopState>> ReadLoopAsync(CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            var areas = await db.Areas.ToListAsync(cancellationToken);
            var thresholds = await db.Thresholds.ToListAsync(cancellationToken);
            var readings = await db.Readings.ToListAsync(cancellationToken);
            var exposures = await db.Exposures.ToListAsync(cancellationToken);
            var alerts = await db.Alerts.ToListAsync(cancellationToken);
            var actuators = await db.Actuators.ToListAsync(cancellationToken);
            var actions = await db.Actions.ToListAsync(cancellationToken);
            return (IReadOnlyList<AreaLoopState>)areas.Select(area =>
            {
                var threshold = thresholds.SingleOrDefault(entry => entry.AreaId == area.AreaId);
                LocalReading? Reading(string measure)
                {
                    var row = readings.SingleOrDefault(entry => entry.AreaId == area.AreaId && entry.Measure == measure);
                    return row is null ? null : ToReading(row);
                }

                var exposure = exposures.SingleOrDefault(entry => entry.AreaId == area.AreaId);
                return new AreaLoopState(
                    area.AreaId,
                    threshold?.Co2LimitPpm,
                    threshold?.NoiseLimitDb,
                    Reading(TelemetryAdmission.CarbonDioxide),
                    Reading(TelemetryAdmission.Noise),
                    Reading(TelemetryAdmission.Presence),
                    exposure is null ? null : new LocalExposure(exposure.AreaId, exposure.Severity, exposure.Highlighted),
                    alerts.Where(entry => entry.AreaId == area.AreaId)
                        .Select(entry => new LocalAlert(entry.Id, entry.AreaId, entry.RaisedAt, entry.WithdrawnAt))
                        .ToArray(),
                    actuators.Where(entry => entry.AreaId == area.AreaId)
                        .Select(entry => new LocalActuator(
                            entry.Id,
                            entry.AreaId,
                            entry.Type,
                            entry.RunState,
                            entry.Mode,
                            entry.LastChangedAt))
                        .ToArray(),
                    actions.Where(entry => entry.AreaId == area.AreaId)
                        .Select(entry => new LocalAction(
                            entry.Id,
                            entry.AreaId,
                            entry.Type,
                            entry.Succeeded,
                            entry.RecordedAt,
                            entry.AttemptedRunState,
                            entry.Synced))
                        .ToArray());
            }).ToArray();
        }, cancellationToken);

    public Task ApplyUpdateAsync(AreaUpdate update, string safetyBody, CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            if (update.WriteExposure)
            {
                var exposure = await db.Exposures.SingleOrDefaultAsync(entry => entry.AreaId == update.AreaId, cancellationToken);
                if (exposure is null)
                {
                    db.Exposures.Add(new LocalExposureRow
                    {
                        AreaId = update.AreaId,
                        Severity = update.Severity,
                        Highlighted = update.Highlighted,
                    });
                }
                else
                {
                    exposure.Severity = update.Severity;
                    exposure.Highlighted = update.Highlighted;
                }
            }

            if (update.WriteAlerts)
            {
                await db.Alerts.Where(entry => entry.AreaId == update.AreaId).ExecuteDeleteAsync(cancellationToken);
                foreach (var alert in update.Alerts)
                {
                    db.Alerts.Add(new LocalAlertRow
                    {
                        Id = alert.Id,
                        AreaId = alert.AreaId,
                        RaisedAt = alert.RaisedAt,
                        WithdrawnAt = alert.WithdrawnAt,
                    });
                }
            }

            foreach (var actuator in update.Actuators)
            {
                var existing = await db.Actuators.SingleOrDefaultAsync(
                    entry => entry.AreaId == actuator.AreaId && entry.Type == actuator.Type,
                    cancellationToken);
                if (existing is null)
                {
                    db.Actuators.Add(new LocalActuatorRow
                    {
                        Id = actuator.Id,
                        AreaId = actuator.AreaId,
                        Type = actuator.Type,
                        RunState = actuator.RunState,
                        Mode = actuator.Mode,
                        LastChangedAt = actuator.LastChangedAt,
                    });
                }
                else
                {
                    existing.RunState = actuator.RunState;
                    existing.Mode = actuator.Mode;
                    existing.LastChangedAt = actuator.LastChangedAt;
                }
            }

            foreach (var action in update.Actions)
            {
                db.Actions.Add(new LocalActionRow
                {
                    Id = action.Id,
                    AreaId = action.AreaId,
                    Type = action.Type,
                    Succeeded = action.Succeeded,
                    RecordedAt = action.RecordedAt,
                    AttemptedRunState = action.AttemptedRunState,
                    Synced = action.Synced,
                });
            }

            await db.Outbound
                .Where(entry => entry.Kind == SafetyKind && entry.AreaId == update.AreaId)
                .ExecuteDeleteAsync(cancellationToken);
            db.Outbound.Add(new OutboundRow
            {
                Kind = SafetyKind,
                Body = safetyBody,
                AreaId = update.AreaId,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }, cancellationToken);

    public Task<OutboundMessage?> PeekSafetyAsync(CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            var row = await db.Outbound
                .Where(entry => entry.Kind == SafetyKind)
                .OrderBy(entry => entry.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return row is null ? null : new OutboundMessage(row.Id, row.Body);
        }, cancellationToken);

    public Task<bool> CompleteSafetyAsync(
        long id,
        string body,
        IReadOnlyList<Guid> syncedActionIds,
        CancellationToken cancellationToken) =>
        Gate(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var deleted = await db.Outbound
                .Where(entry => entry.Id == id && entry.Body == body)
                .ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0)
            {
                return false;
            }

            if (syncedActionIds.Count > 0)
            {
                var actions = await db.Actions
                    .Where(entry => syncedActionIds.Contains(entry.Id))
                    .ToListAsync(cancellationToken);
                foreach (var action in actions)
                {
                    action.Synced = true;
                }

                await db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }, cancellationToken);

    private async Task Gate(Func<PlantLocalDbContext, Task> action, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var db = await OpenAsync(cancellationToken);
            await action(db);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> Gate<T>(Func<PlantLocalDbContext, Task<T>> action, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var db = await OpenAsync(cancellationToken);
            return await action(db);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<PlantLocalDbContext> OpenAsync(CancellationToken cancellationToken)
    {
        if (!_ready && !string.IsNullOrEmpty(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        var db = new PlantLocalDbContext(_options);
        if (_ready)
        {
            return db;
        }

        try
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
            _ready = true;
            return db;
        }
        catch
        {
            await db.DisposeAsync();
            throw;
        }
    }

    private static async Task UpsertReadingAsync(
        PlantLocalDbContext db,
        LocalReading reading,
        CancellationToken cancellationToken)
    {
        var existing = await db.Readings.SingleOrDefaultAsync(
            entry => entry.AreaId == reading.AreaId && entry.Measure == reading.Measure,
            cancellationToken);
        if (existing is null)
        {
            db.Readings.Add(new LocalReadingRow
            {
                AreaId = reading.AreaId,
                Measure = reading.Measure,
                DeviceId = reading.DeviceId,
                Ppm = reading.Ppm,
                Decibels = reading.Decibels,
                State = reading.State,
                RecordedAt = reading.RecordedAt,
            });
            return;
        }

        if (reading.RecordedAt < existing.RecordedAt)
        {
            return;
        }

        existing.DeviceId = reading.DeviceId;
        existing.Ppm = reading.Ppm;
        existing.Decibels = reading.Decibels;
        existing.State = reading.State;
        existing.RecordedAt = reading.RecordedAt;
    }

    private static LocalReading ToReading(LocalReadingRow row) =>
        new(row.AreaId, row.Measure, row.DeviceId, row.Ppm, row.Decibels, row.State, row.RecordedAt);

    private static string? DirectoryOf(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return null;
        }

        return Path.GetDirectoryName(Path.GetFullPath(dataSource));
    }
}
