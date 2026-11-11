using Microsoft.EntityFrameworkCore;
using SafetyActuation.Domain.Entities;
using SafetyActuation.Domain.Interfaces;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class EfSafetyRecordStore : ISafetyRecordStore
{
    private readonly SafetyActuationDbContext _db;

    public EfSafetyRecordStore(SafetyActuationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EnvironmentalAlert>> ListActiveAlertsAsync(CancellationToken cancellationToken) =>
        await _db.Alerts
            .Where(alert => alert.WithdrawnAt == null)
            .OrderByDescending(alert => alert.RaisedAt)
            .ThenByDescending(alert => alert.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EnvironmentalAlert>> ListAlertsByAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken) =>
        await _db.Alerts
            .Where(alert => alert.AreaId == areaId)
            .OrderBy(alert => alert.RaisedAt)
            .ThenBy(alert => alert.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ActuatorAction>> ListActionsByAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken) =>
        await _db.Actions
            .Where(action => action.AreaId == areaId)
            .OrderBy(action => action.RecordedAt)
            .ThenBy(action => action.Id)
            .ToListAsync(cancellationToken);
}
