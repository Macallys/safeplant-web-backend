using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Application;

namespace PlantMonitoring.Application.Services;

public sealed class DeviceService : IDeviceService
{
    private const int DeviceIdMaxLength = 64;

    private readonly IAreaStore _areas;
    private readonly IThresholdStore _thresholds;
    private readonly IDeviceStore _devices;

    public DeviceService(IAreaStore areas, IThresholdStore thresholds, IDeviceStore devices)
    {
        _areas = areas;
        _thresholds = thresholds;
        _devices = devices;
    }

    public async Task<AreaDevice> AssociateAsync(
        string role,
        string channel,
        string? areaId,
        string? deviceId,
        string? kind,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var parsedDeviceId = RequireDeviceId(deviceId);
        var parsedKind = RequireKind(kind);
        var area = await FindAreaAsync(areaId, cancellationToken);

        if (await _devices.FindByDeviceIdAsync(parsedDeviceId, cancellationToken) is not null)
        {
            throw new AppException("device_already_exists", 409);
        }

        var device = new AreaDevice(Guid.NewGuid(), area.Id, parsedDeviceId, parsedKind);
        await _devices.AddAsync(device, cancellationToken);
        return device;
    }

    public async Task<AreaSetupSheet> GetSetupAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var area = await FindAreaAsync(areaId, cancellationToken);
        var thresholds = await _thresholds.FindByAreaIdAsync(area.Id, cancellationToken);
        var devices = await _devices.ListByAreaIdAsync(area.Id, cancellationToken);
        return new AreaSetupSheet(
            area.Id,
            area.Name,
            area.Location,
            thresholds?.Co2LimitPpm,
            thresholds?.NoiseLimitDb,
            devices);
    }

    private async Task<IndustrialArea> FindAreaAsync(string? areaId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(areaId, out var id))
        {
            throw new AppException("area_not_found", 404);
        }

        var area = await _areas.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new AppException("area_not_found", 404);
        }

        return area;
    }

    private static void RequireSupervisor(string role, string channel)
    {
        if (role != "Supervisor" || channel != "Mobile")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }

    private static string RequireDeviceId(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new AppException("invalid_area", 422);
        }

        var trimmed = deviceId.Trim();
        if (trimmed.Length > DeviceIdMaxLength)
        {
            throw new AppException("invalid_area", 422);
        }

        return trimmed;
    }

    private static string RequireKind(string? kind)
    {
        if (kind is not ("Sensor" or "Actuator"))
        {
            throw new AppException("invalid_area", 422);
        }

        return kind;
    }
}
