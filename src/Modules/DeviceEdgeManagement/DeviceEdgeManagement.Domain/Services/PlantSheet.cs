namespace DeviceEdgeManagement.Domain.Services;

public sealed record PlantAreaReplica(Guid Id, string Name, string Location, DateTimeOffset UpdatedAt);

public sealed record PlantThresholdReplica(Guid AreaId, int Co2LimitPpm, int NoiseLimitDb, DateTimeOffset UpdatedAt);

public sealed record PlantDeviceReplica(string DeviceId, Guid AreaId, string Kind);

public sealed record PlantCredentialReplica(
    string DeviceId,
    string SecretHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt);

public sealed record PlantSheet(
    IReadOnlyList<PlantAreaReplica> Areas,
    IReadOnlyList<PlantThresholdReplica> Thresholds,
    IReadOnlyList<PlantDeviceReplica> Devices,
    IReadOnlyList<PlantCredentialReplica> Credentials);

public interface IPlantSheetService
{
    void RequirePlantKey(string? presentedKey);

    Task<PlantSheet> ReadAsync(CancellationToken cancellationToken);
}
