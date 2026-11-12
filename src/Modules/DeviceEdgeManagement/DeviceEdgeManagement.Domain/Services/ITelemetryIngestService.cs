namespace DeviceEdgeManagement.Domain.Services;

public static class TelemetryHeaders
{
    public const string PlantKey = "X-Plant-Key";
}

public sealed record TelemetryIngest(
    string? DeviceId,
    string? Secret,
    string? Measure,
    int? Ppm,
    int? Decibels,
    string? State,
    DateTimeOffset? RecordedAt);

public interface ITelemetryIngestService
{
    void RequirePlantKey(string? presentedKey);

    Task AcceptAsync(TelemetryIngest reading, CancellationToken cancellationToken);
}
