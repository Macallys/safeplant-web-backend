using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class EfDeviceStore : IDeviceStore
{
    private readonly PlantMonitoringDbContext _db;

    public EfDeviceStore(PlantMonitoringDbContext db)
    {
        _db = db;
    }

    public Task<AreaDevice?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken) =>
        _db.Devices.SingleOrDefaultAsync(entry => entry.DeviceId == deviceId, cancellationToken);

    public async Task<IReadOnlyList<AreaDevice>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Devices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AreaDevice>> ListByAreaIdAsync(Guid areaId, CancellationToken cancellationToken) =>
        await _db.Devices
            .Where(entry => entry.AreaId == areaId)
            .OrderBy(entry => entry.DeviceId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AreaDevice device, CancellationToken cancellationToken)
    {
        await _db.Devices.AddAsync(device, cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new AppException("device_already_exists", 409);
        }
    }

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
