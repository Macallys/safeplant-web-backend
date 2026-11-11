using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class EfAreaSafetyStatus : IAreaSafetyStatus
{
    private readonly SafetyActuationDbContext _db;

    public EfAreaSafetyStatus(SafetyActuationDbContext db)
    {
        _db = db;
    }

    public AreaSafetySnapshot ForArea(Guid areaId)
    {
        var exposure = _db.Exposures.SingleOrDefault(entry => entry.AreaId == areaId);
        var alertActive = _db.Alerts.Any(alert => alert.AreaId == areaId && alert.WithdrawnAt == null);
        return new AreaSafetySnapshot(
            exposure?.Severity ?? "None",
            alertActive,
            exposure?.Highlighted ?? false);
    }

    public IReadOnlyList<AreaAlert> Alerts(Guid areaId, DateTimeOffset from, DateTimeOffset to) =>
        _db.Alerts
            .Where(alert => alert.AreaId == areaId
                && alert.RaisedAt <= to
                && (alert.WithdrawnAt == null || alert.WithdrawnAt >= from))
            .OrderBy(alert => alert.RaisedAt)
            .ThenBy(alert => alert.Id)
            .AsEnumerable()
            .Select(alert => new AreaAlert(alert.Id, alert.AreaId, alert.RaisedAt, alert.WithdrawnAt))
            .ToArray();

    public IReadOnlyList<AreaAction> Actions(Guid areaId, DateTimeOffset from, DateTimeOffset to) =>
        _db.Actions
            .Where(action => action.AreaId == areaId && action.RecordedAt >= from && action.RecordedAt <= to)
            .OrderBy(action => action.RecordedAt)
            .ThenBy(action => action.Id)
            .AsEnumerable()
            .Select(action => new AreaAction(action.Id, action.AreaId, action.Type, action.Succeeded, action.RecordedAt))
            .ToArray();
}
