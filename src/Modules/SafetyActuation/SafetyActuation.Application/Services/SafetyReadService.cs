using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Application.Services;

public sealed class SafetyReadService : ISafetyReadService
{
    private readonly IAreaStore _areas;
    private readonly ISafetyRecordStore _records;

    public SafetyReadService(IAreaStore areas, ISafetyRecordStore records)
    {
        _areas = areas;
        _records = records;
    }

    public async Task<IReadOnlyList<AlertItem>> ActiveAlertsAsync(
        string role,
        string channel,
        CancellationToken cancellationToken)
    {
        RequireReader(role, channel);
        var alerts = await _records.ListActiveAlertsAsync(cancellationToken);
        return alerts.Select(alert => new AlertItem(alert.Id, alert.AreaId, alert.RaisedAt, alert.WithdrawnAt)).ToArray();
    }

    public async Task<IReadOnlyList<AlertItem>> ExposuresAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken)
    {
        RequireReader(role, channel);
        var area = await FindAreaAsync(areaId, cancellationToken);
        var alerts = await _records.ListAlertsByAreaAsync(area.Id, cancellationToken);
        return alerts.Select(alert => new AlertItem(alert.Id, alert.AreaId, alert.RaisedAt, alert.WithdrawnAt)).ToArray();
    }

    public async Task<IReadOnlyList<ActionItem>> ActionsAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken)
    {
        RequireReader(role, channel);
        var area = await FindAreaAsync(areaId, cancellationToken);
        var actions = await _records.ListActionsByAreaAsync(area.Id, cancellationToken);
        return actions
            .Select(action => new ActionItem(action.Id, action.AreaId, action.Type, action.Succeeded, action.RecordedAt))
            .ToArray();
    }

    private async Task<PlantMonitoring.Domain.Entities.IndustrialArea> FindAreaAsync(
        string? areaId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(areaId, out var id))
        {
            throw new AppException("area_not_found", 404);
        }

        var area = await _areas.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new AppException("area_not_found", 404);
        }

        return area;
    }

    private static void RequireReader(string role, string channel)
    {
        var allowed = (role == "Supervisor" && channel == "Mobile")
            || (role == "PlantManager" && channel == "Web");
        if (!allowed)
        {
            throw new AppException("role_not_allowed", 403);
        }
    }
}
