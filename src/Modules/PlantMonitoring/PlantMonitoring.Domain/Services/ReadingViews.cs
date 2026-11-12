namespace PlantMonitoring.Domain.Services;

public sealed record CarbonDioxidePoint(string DeviceId, int Ppm, DateTimeOffset RecordedAt);

public sealed record NoisePoint(string DeviceId, int Db, DateTimeOffset RecordedAt);

public sealed record PresencePoint(string DeviceId, string State, DateTimeOffset RecordedAt);

public sealed record LatestReadings(
    Guid AreaId,
    CarbonDioxidePoint? CarbonDioxide,
    NoisePoint? Noise,
    PresencePoint? Presence);

public sealed record DashboardItem(
    Guid AreaId,
    string Name,
    int? Co2Ppm,
    int? NoiseDb,
    string? PresenceState,
    string Severity,
    bool AlertActive,
    bool Highlighted,
    int? Co2LimitPpm,
    int? NoiseLimitDb);

public sealed record CarbonDioxideHistoryItem(Guid Id, string DeviceId, int Ppm, DateTimeOffset RecordedAt);

public sealed record NoiseHistoryItem(Guid Id, string DeviceId, int Db, DateTimeOffset RecordedAt);

public sealed record PresenceHistoryItem(Guid Id, string DeviceId, string State, DateTimeOffset RecordedAt);

public sealed record AreaHistory(
    Guid AreaId,
    IReadOnlyList<CarbonDioxideHistoryItem> CarbonDioxideReadings,
    IReadOnlyList<NoiseHistoryItem> NoiseReadings,
    IReadOnlyList<PresenceHistoryItem> PresenceChanges,
    IReadOnlyList<AreaAlert> Alerts,
    IReadOnlyList<AreaAction> Actions);
