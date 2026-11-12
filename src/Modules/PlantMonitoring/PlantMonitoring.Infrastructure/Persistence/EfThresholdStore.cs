using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class EfThresholdStore : IThresholdStore
{
    private readonly PlantMonitoringDbContext _db;

    public EfThresholdStore(PlantMonitoringDbContext db)
    {
        _db = db;
    }

    public Task<AreaThresholds?> FindByAreaIdAsync(Guid areaId, CancellationToken cancellationToken) =>
        _db.Thresholds.SingleOrDefaultAsync(entry => entry.AreaId == areaId, cancellationToken);

    public async Task<IReadOnlyList<AreaThresholds>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Thresholds.OrderBy(entry => entry.AreaId).ToListAsync(cancellationToken);

    public async Task<AreaThresholds> AddAsync(AreaThresholds thresholds, CancellationToken cancellationToken)
    {
        await _db.Thresholds.AddAsync(thresholds, cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return thresholds;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _db.Entry(thresholds).State = EntityState.Detached;
            var existing = await _db.Thresholds.SingleAsync(
                entry => entry.AreaId == thresholds.AreaId,
                cancellationToken);
            existing.Replace(thresholds.Co2LimitPpm, thresholds.NoiseLimitDb, thresholds.UpdatedAt);
            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (var current = exception.InnerException; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}
