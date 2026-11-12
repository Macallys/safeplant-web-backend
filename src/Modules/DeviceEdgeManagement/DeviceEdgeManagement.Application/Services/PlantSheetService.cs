using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;

namespace DeviceEdgeManagement.Application.Services;

public sealed class PlantSheetService : IPlantSheetService
{
    private readonly string _plantKey;
    private readonly IAreaStore _areas;
    private readonly IThresholdStore _thresholds;
    private readonly IDeviceStore _devices;
    private readonly IDeviceCredentialStore _credentials;

    public PlantSheetService(
        string plantKey,
        IAreaStore areas,
        IThresholdStore thresholds,
        IDeviceStore devices,
        IDeviceCredentialStore credentials)
    {
        _plantKey = plantKey;
        _areas = areas;
        _thresholds = thresholds;
        _devices = devices;
        _credentials = credentials;
    }

    public void RequirePlantKey(string? presentedKey)
    {
        if (!PlantKeys.Matches(_plantKey, presentedKey))
        {
            throw new AppException("unauthorized", 401);
        }
    }

    public async Task<PlantSheet> ReadAsync(CancellationToken cancellationToken)
    {
        var areas = await _areas.ListAsync(cancellationToken);
        var thresholds = await _thresholds.ListAsync(cancellationToken);
        var devices = await _devices.ListAsync(cancellationToken);
        var credentials = await _credentials.ListAsync(cancellationToken);
        return new PlantSheet(
            areas.Select(area => new PlantAreaReplica(area.Id, area.Name, area.Location, area.UpdatedAt)).ToArray(),
            thresholds.Select(threshold => new PlantThresholdReplica(
                threshold.AreaId,
                threshold.Co2LimitPpm,
                threshold.NoiseLimitDb,
                threshold.UpdatedAt)).ToArray(),
            devices.Select(device => new PlantDeviceReplica(device.DeviceId, device.AreaId, device.Kind)).ToArray(),
            credentials.Select(credential => new PlantCredentialReplica(
                credential.DeviceId,
                credential.SecretHash,
                credential.ExpiresAt,
                credential.RevokedAt)).ToArray());
    }
}
