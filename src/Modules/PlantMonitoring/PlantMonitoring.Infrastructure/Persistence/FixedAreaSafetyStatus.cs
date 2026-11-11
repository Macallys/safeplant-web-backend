using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class FixedAreaSafetyStatus : IAreaSafetyStatus
{
    public AreaSafetySnapshot ForArea(Guid areaId) => new("None", false, false);

    public IReadOnlyList<AreaAlert> Alerts(Guid areaId, DateTimeOffset from, DateTimeOffset to) => [];

    public IReadOnlyList<AreaAction> Actions(Guid areaId, DateTimeOffset from, DateTimeOffset to) => [];
}
