using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Services;

public interface IDeviceService
{
    Task<AreaDevice> AssociateAsync(
        string role,
        string channel,
        string? areaId,
        string? deviceId,
        string? kind,
        CancellationToken cancellationToken);

    Task<AreaSetupSheet> GetSetupAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken);
}
