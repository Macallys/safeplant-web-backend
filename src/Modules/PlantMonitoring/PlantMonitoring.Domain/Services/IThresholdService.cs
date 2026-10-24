using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Services;

public interface IThresholdService
{
    Task<AreaThresholds> ConfigureAsync(
        string role,
        string channel,
        string? areaId,
        decimal? co2LimitPpm,
        decimal? noiseLimitDb,
        CancellationToken cancellationToken);
}
