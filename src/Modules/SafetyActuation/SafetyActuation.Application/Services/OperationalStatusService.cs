using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Domain.Entities;
using SafetyActuation.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Application.Services;

public sealed class OperationalStatusService : IOperationalStatusService
{
    private static readonly string[] ActuatorOrder = ["Extractor", "Siren", "Barrier"];

    private readonly IAreaStore _areas;
    private readonly IReadingStore _readings;
    private readonly IAreaSafetyStatus _safety;
    private readonly IActuatorStore _actuators;

    public OperationalStatusService(
        IAreaStore areas,
        IReadingStore readings,
        IAreaSafetyStatus safety,
        IActuatorStore actuators)
    {
        _areas = areas;
        _readings = readings;
        _safety = safety;
        _actuators = actuators;
    }

    public async Task<IReadOnlyList<OperationalStatus>> ListAsync(
        string role,
        string channel,
        CancellationToken cancellationToken)
    {
        RequireReader(role, channel);
        var areas = await _areas.ListAsync(cancellationToken);
        var items = new List<OperationalStatus>(areas.Count);
        foreach (var area in areas)
        {
            items.Add(await BuildAsync(area, cancellationToken));
        }

        return items;
    }

    public async Task<OperationalStatus> GetAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken)
    {
        RequireReader(role, channel);
        var area = await FindAreaAsync(areaId, cancellationToken);
        return await BuildAsync(area, cancellationToken);
    }

    private async Task<OperationalStatus> BuildAsync(IndustrialArea area, CancellationToken cancellationToken)
    {
        var carbonDioxide = await _readings.LatestCarbonDioxideAsync(area.Id, cancellationToken);
        var noise = await _readings.LatestNoiseAsync(area.Id, cancellationToken);
        var presence = await _readings.LatestPresenceAsync(area.Id, cancellationToken);
        var safety = _safety.ForArea(area.Id);
        var actuators = await _actuators.ListByAreaAsync(area.Id, cancellationToken);

        return new OperationalStatus(
            area.Id,
            area.Name,
            carbonDioxide?.Ppm,
            noise?.Db,
            presence?.State,
            safety.Severity,
            safety.AlertActive,
            safety.Highlighted,
            Order(actuators));
    }

    private async Task<IndustrialArea> FindAreaAsync(string? areaId, CancellationToken cancellationToken)
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

    private static IReadOnlyList<ActuatorPoint> Order(IReadOnlyList<ActuatorState> actuators) =>
        actuators
            .OrderBy(actuator => Rank(actuator.Type))
            .ThenBy(actuator => actuator.Type, StringComparer.Ordinal)
            .Select(actuator => new ActuatorPoint(actuator.Type, actuator.RunState, actuator.Mode, actuator.LastChangedAt))
            .ToArray();

    private static int Rank(string type)
    {
        var index = Array.IndexOf(ActuatorOrder, type);
        return index < 0 ? ActuatorOrder.Length : index;
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
