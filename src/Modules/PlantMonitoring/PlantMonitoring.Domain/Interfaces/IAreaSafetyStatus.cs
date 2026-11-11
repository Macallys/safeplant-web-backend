using PlantMonitoring.Domain.Services;

namespace PlantMonitoring.Domain.Interfaces;

public interface IAreaSafetyStatus
{
    AreaSafetySnapshot ForArea(Guid areaId);

    IReadOnlyList<AreaAlert> Alerts(Guid areaId, DateTimeOffset from, DateTimeOffset to);

    IReadOnlyList<AreaAction> Actions(Guid areaId, DateTimeOffset from, DateTimeOffset to);
}
