using PlantMonitoring.Domain.Services;

namespace PlantMonitoring.Interface.Assemblers;

public sealed record CarbonDioxideLatestResponse(string DeviceId, int Ppm, DateTimeOffset RecordedAt);

public sealed record NoiseLatestResponse(string DeviceId, int Db, DateTimeOffset RecordedAt);

public sealed record PresenceLatestResponse(string DeviceId, string State, DateTimeOffset RecordedAt);

public sealed record LatestReadingsResponse(
    string AreaId,
    CarbonDioxideLatestResponse? CarbonDioxide,
    NoiseLatestResponse? Noise,
    PresenceLatestResponse? Presence);

public sealed record DashboardItemResponse(
    string AreaId,
    string Name,
    int? Co2Ppm,
    int? NoiseDb,
    string? PresenceState,
    string Severity,
    bool AlertActive,
    bool Highlighted,
    int? Co2LimitPpm,
    int? NoiseLimitDb);

public sealed record DashboardResponse(IReadOnlyList<DashboardItemResponse> Items);

public sealed record CarbonDioxideHistoryResponse(string Id, string DeviceId, int Ppm, DateTimeOffset RecordedAt);

public sealed record NoiseHistoryResponse(string Id, string DeviceId, int Db, DateTimeOffset RecordedAt);

public sealed record PresenceHistoryResponse(string Id, string DeviceId, string State, DateTimeOffset RecordedAt);

public sealed record AlertHistoryResponse(string Id, string AreaId, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record ActionHistoryResponse(string Id, string AreaId, string Type, bool Succeeded, DateTimeOffset RecordedAt);

public sealed record AreaHistoryResponse(
    string AreaId,
    IReadOnlyList<CarbonDioxideHistoryResponse> CarbonDioxideReadings,
    IReadOnlyList<NoiseHistoryResponse> NoiseReadings,
    IReadOnlyList<PresenceHistoryResponse> PresenceChanges,
    IReadOnlyList<AlertHistoryResponse> Alerts,
    IReadOnlyList<ActionHistoryResponse> Actions);

public static class ReadingAssembler
{
    public static LatestReadingsResponse ToLatest(LatestReadings readings) =>
        new(
            readings.AreaId.ToString(),
            readings.CarbonDioxide is null
                ? null
                : new CarbonDioxideLatestResponse(
                    readings.CarbonDioxide.DeviceId,
                    readings.CarbonDioxide.Ppm,
                    readings.CarbonDioxide.RecordedAt),
            readings.Noise is null
                ? null
                : new NoiseLatestResponse(readings.Noise.DeviceId, readings.Noise.Db, readings.Noise.RecordedAt),
            readings.Presence is null
                ? null
                : new PresenceLatestResponse(
                    readings.Presence.DeviceId,
                    readings.Presence.State,
                    readings.Presence.RecordedAt));

    public static DashboardResponse ToDashboard(IReadOnlyList<DashboardItem> items) =>
        new(items.Select(item => new DashboardItemResponse(
            item.AreaId.ToString(),
            item.Name,
            item.Co2Ppm,
            item.NoiseDb,
            item.PresenceState,
            item.Severity,
            item.AlertActive,
            item.Highlighted,
            item.Co2LimitPpm,
            item.NoiseLimitDb)).ToArray());

    public static AreaHistoryResponse ToHistory(AreaHistory history) =>
        new(
            history.AreaId.ToString(),
            history.CarbonDioxideReadings
                .Select(reading => new CarbonDioxideHistoryResponse(
                    reading.Id.ToString(),
                    reading.DeviceId,
                    reading.Ppm,
                    reading.RecordedAt))
                .ToArray(),
            history.NoiseReadings
                .Select(reading => new NoiseHistoryResponse(reading.Id.ToString(), reading.DeviceId, reading.Db, reading.RecordedAt))
                .ToArray(),
            history.PresenceChanges
                .Select(reading => new PresenceHistoryResponse(
                    reading.Id.ToString(),
                    reading.DeviceId,
                    reading.State,
                    reading.RecordedAt))
                .ToArray(),
            history.Alerts
                .Select(alert => new AlertHistoryResponse(
                    alert.Id.ToString(),
                    alert.AreaId.ToString(),
                    alert.RaisedAt,
                    alert.WithdrawnAt))
                .ToArray(),
            history.Actions
                .Select(action => new ActionHistoryResponse(
                    action.Id.ToString(),
                    action.AreaId.ToString(),
                    action.Type,
                    action.Succeeded,
                    action.RecordedAt))
                .ToArray());
}
