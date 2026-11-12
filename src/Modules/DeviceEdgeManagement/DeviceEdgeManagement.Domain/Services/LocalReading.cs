namespace DeviceEdgeManagement.Domain.Services;

public sealed record LocalReading(
    Guid AreaId,
    string Measure,
    string DeviceId,
    int? Ppm,
    int? Decibels,
    string? State,
    DateTimeOffset RecordedAt);

public sealed record OutboundMessage(long Id, string Body);
