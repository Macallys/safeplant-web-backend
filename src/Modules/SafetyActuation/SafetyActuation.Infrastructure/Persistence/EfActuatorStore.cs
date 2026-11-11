using Microsoft.EntityFrameworkCore;
using SafetyActuation.Domain.Entities;
using SafetyActuation.Domain.Interfaces;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class EfActuatorStore : IActuatorStore
{
    private readonly SafetyActuationDbContext _db;

    public EfActuatorStore(SafetyActuationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ActuatorState>> ListByAreaAsync(Guid areaId, CancellationToken cancellationToken) =>
        await _db.Actuators.Where(actuator => actuator.AreaId == areaId).ToListAsync(cancellationToken);
}
