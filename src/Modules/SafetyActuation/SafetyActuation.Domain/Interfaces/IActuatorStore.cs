using SafetyActuation.Domain.Entities;

namespace SafetyActuation.Domain.Interfaces;

public interface IActuatorStore
{
    Task<IReadOnlyList<ActuatorState>> ListByAreaAsync(Guid areaId, CancellationToken cancellationToken);
}
