using Microsoft.EntityFrameworkCore;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class EfReadingStore : IReadingStore
{
    private readonly PlantMonitoringDbContext _db;

    public EfReadingStore(PlantMonitoringDbContext db)
    {
        _db = db;
    }

    public Task<CarbonDioxideReading?> LatestCarbonDioxideAsync(Guid areaId, CancellationToken cancellationToken) =>
        _db.CarbonDioxideReadings
            .Where(reading => reading.AreaId == areaId)
            .OrderByDescending(reading => reading.RecordedAt)
            .ThenByDescending(reading => reading.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<NoiseReading?> LatestNoiseAsync(Guid areaId, CancellationToken cancellationToken) =>
        _db.NoiseReadings
            .Where(reading => reading.AreaId == areaId)
            .OrderByDescending(reading => reading.RecordedAt)
            .ThenByDescending(reading => reading.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<PresenceChange?> LatestPresenceAsync(Guid areaId, CancellationToken cancellationToken) =>
        _db.PresenceChanges
            .Where(reading => reading.AreaId == areaId)
            .OrderByDescending(reading => reading.RecordedAt)
            .ThenByDescending(reading => reading.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<CarbonDioxideReading>> CarbonDioxideBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await _db.CarbonDioxideReadings
            .Where(reading => reading.AreaId == areaId && reading.RecordedAt >= from && reading.RecordedAt <= to)
            .OrderBy(reading => reading.RecordedAt)
            .ThenBy(reading => reading.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NoiseReading>> NoiseBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await _db.NoiseReadings
            .Where(reading => reading.AreaId == areaId && reading.RecordedAt >= from && reading.RecordedAt <= to)
            .OrderBy(reading => reading.RecordedAt)
            .ThenBy(reading => reading.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PresenceChange>> PresenceBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await _db.PresenceChanges
            .Where(reading => reading.AreaId == areaId && reading.RecordedAt >= from && reading.RecordedAt <= to)
            .OrderBy(reading => reading.RecordedAt)
            .ThenBy(reading => reading.Id)
            .ToListAsync(cancellationToken);
}
