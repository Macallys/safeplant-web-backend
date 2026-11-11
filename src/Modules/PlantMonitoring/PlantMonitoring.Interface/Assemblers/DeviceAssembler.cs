using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Services;

namespace PlantMonitoring.Interface.Assemblers;

public sealed record DeviceBody(string? DeviceId, string? Kind);

public sealed record DeviceResponse(string Id, string AreaId, string DeviceId, string Kind);

public sealed record SetupDeviceResponse(string Id, string DeviceId, string Kind);

public sealed record SetupResponse(
    string Id,
    string Name,
    string Location,
    int? Co2LimitPpm,
    int? NoiseLimitDb,
    IReadOnlyList<SetupDeviceResponse> Devices);

public static class DeviceAssembler
{
    public static DeviceResponse ToResponse(AreaDevice device) =>
        new(device.Id.ToString(), device.AreaId.ToString(), device.DeviceId, device.Kind);

    public static SetupResponse ToSetup(AreaSetupSheet sheet) =>
        new(
            sheet.Id.ToString(),
            sheet.Name,
            sheet.Location,
            sheet.Co2LimitPpm,
            sheet.NoiseLimitDb,
            sheet.Devices.Select(device => new SetupDeviceResponse(device.Id.ToString(), device.DeviceId, device.Kind)).ToArray());
}
