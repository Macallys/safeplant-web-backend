using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Interfaces;

public interface IThresholdStore
{
    Task<AreaThresholds?> FindByAreaIdAsync(Guid areaId, CancellationToken cancellationToken);

    Task<AreaThresholds> AddAsync(AreaThresholds thresholds, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
