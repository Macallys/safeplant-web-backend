using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class EfAreaStore : IAreaStore
{
    private readonly PlantMonitoringDbContext _db;

    public EfAreaStore(PlantMonitoringDbContext db)
    {
        _db = db;
    }

    public Task<IndustrialArea?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Areas.SingleOrDefaultAsync(area => area.Id == id, cancellationToken);

    public Task<IndustrialArea?> FindByNameAsync(string name, CancellationToken cancellationToken) =>
        _db.Areas.SingleOrDefaultAsync(area => area.Name.ToLower() == name.ToLower(), cancellationToken);

    public async Task AddAsync(IndustrialArea area, CancellationToken cancellationToken) =>
        await _db.Areas.AddAsync(area, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new AppException("area_name_duplicate", 409);
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
