using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Interfaces;

public interface IAreaStore
{
    Task<IndustrialArea?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IndustrialArea?> FindByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(IndustrialArea area, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
