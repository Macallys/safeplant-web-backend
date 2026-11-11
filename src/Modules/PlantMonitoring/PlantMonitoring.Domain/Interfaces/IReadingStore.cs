using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Interfaces;

public interface IReadingStore
{
    Task<CarbonDioxideReading?> LatestCarbonDioxideAsync(Guid areaId, CancellationToken cancellationToken);

    Task<NoiseReading?> LatestNoiseAsync(Guid areaId, CancellationToken cancellationToken);

    Task<PresenceChange?> LatestPresenceAsync(Guid areaId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CarbonDioxideReading>> CarbonDioxideBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NoiseReading>> NoiseBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PresenceChange>> PresenceBetweenAsync(
        Guid areaId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
