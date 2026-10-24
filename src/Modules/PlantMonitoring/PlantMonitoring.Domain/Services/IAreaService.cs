using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Services;

public interface IAreaService
{
    Task<IndustrialArea> CreateAsync(
        string role,
        string channel,
        string? name,
        string? location,
        CancellationToken cancellationToken);

    Task<IndustrialArea> UpdateAsync(
        string role,
        string channel,
        string? areaId,
        string? name,
        string? location,
        CancellationToken cancellationToken);
}
