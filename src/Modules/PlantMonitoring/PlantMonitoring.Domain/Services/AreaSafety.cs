namespace PlantMonitoring.Domain.Services;

public sealed record AreaSafetySnapshot(string Severity, bool AlertActive, bool Highlighted);

public sealed record AreaAlert(Guid Id, Guid AreaId, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record AreaAction(Guid Id, Guid AreaId, string Type, bool Succeeded, DateTimeOffset RecordedAt);
