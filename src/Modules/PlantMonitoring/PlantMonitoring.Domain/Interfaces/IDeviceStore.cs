using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Interfaces;

public interface IDeviceStore
{
    Task<AreaDevice?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AreaDevice>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AreaDevice>> ListByAreaIdAsync(Guid areaId, CancellationToken cancellationToken);

    Task AddAsync(AreaDevice device, CancellationToken cancellationToken);
}
