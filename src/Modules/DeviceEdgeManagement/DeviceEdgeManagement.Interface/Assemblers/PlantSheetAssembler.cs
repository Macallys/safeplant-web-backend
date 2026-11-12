using DeviceEdgeManagement.Domain.Services;

namespace DeviceEdgeManagement.Interface.Assemblers;

public sealed record PlantSheetResponse(
    IReadOnlyList<PlantAreaItem> Areas,
    IReadOnlyList<PlantThresholdItem> Thresholds,
    IReadOnlyList<PlantDeviceItem> Devices,
    IReadOnlyList<PlantCredentialItem> Credentials);

public sealed record PlantAreaItem(Guid Id, string Name, string Location, DateTimeOffset UpdatedAt);

public sealed record PlantThresholdItem(Guid AreaId, int Co2LimitPpm, int NoiseLimitDb, DateTimeOffset UpdatedAt);

public sealed record PlantDeviceItem(string DeviceId, Guid AreaId, string Kind);

public sealed record PlantCredentialItem(string DeviceId, string SecretHash, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt);

public static class PlantSheetAssembler
{
    public static PlantSheetResponse ToResponse(PlantSheet sheet) =>
        new(
            sheet.Areas.Select(area => new PlantAreaItem(area.Id, area.Name, area.Location, area.UpdatedAt)).ToArray(),
            sheet.Thresholds.Select(threshold => new PlantThresholdItem(
                threshold.AreaId,
                threshold.Co2LimitPpm,
                threshold.NoiseLimitDb,
                threshold.UpdatedAt)).ToArray(),
            sheet.Devices.Select(device => new PlantDeviceItem(device.DeviceId, device.AreaId, device.Kind)).ToArray(),
            sheet.Credentials.Select(credential => new PlantCredentialItem(
                credential.DeviceId,
                credential.SecretHash,
                credential.ExpiresAt,
                credential.RevokedAt)).ToArray());
}
