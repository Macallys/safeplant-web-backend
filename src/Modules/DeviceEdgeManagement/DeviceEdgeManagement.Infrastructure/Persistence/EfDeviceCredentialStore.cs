using DeviceEdgeManagement.Domain.Entities;
using DeviceEdgeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel.Application;

namespace DeviceEdgeManagement.Infrastructure.Persistence;

public sealed class EfDeviceCredentialStore : IDeviceCredentialStore
{
    private readonly DeviceEdgeDbContext _db;

    public EfDeviceCredentialStore(DeviceEdgeDbContext db)
    {
        _db = db;
    }

    public Task<DeviceCredential?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken) =>
        _db.Credentials.SingleOrDefaultAsync(entry => entry.DeviceId == deviceId, cancellationToken);

    public async Task<IReadOnlyList<DeviceCredential>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Credentials.OrderBy(entry => entry.DeviceId).ToListAsync(cancellationToken);

    public async Task AddAsync(DeviceCredential credential, CancellationToken cancellationToken)
    {
        await _db.Credentials.AddAsync(credential, cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new AppException("device_already_exists", 409);
        }
    }

    public async Task RemoveAsync(DeviceCredential credential, CancellationToken cancellationToken)
    {
        _db.Credentials.Remove(credential);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken) =>
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
