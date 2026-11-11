using SafetyActuation.Domain.Entities;

namespace SafetyActuation.Domain.Interfaces;

public interface ISafetyRecordStore
{
    Task<IReadOnlyList<EnvironmentalAlert>> ListActiveAlertsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<EnvironmentalAlert>> ListAlertsByAreaAsync(Guid areaId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ActuatorAction>> ListActionsByAreaAsync(Guid areaId, CancellationToken cancellationToken);
}
