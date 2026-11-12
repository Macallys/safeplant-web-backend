namespace SafetyActuation.Domain.Services;

public sealed record SafetyAlertSync(Guid Id, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record SafetyActuatorSync(
    Guid Id,
    string Type,
    string RunState,
    string Mode,
    DateTimeOffset LastChangedAt);

public sealed record SafetyActionSync(Guid Id, string Type, bool Succeeded, DateTimeOffset RecordedAt);

public sealed record SafetySync(
    Guid AreaId,
    string? Severity,
    bool Highlighted,
    IReadOnlyList<SafetyAlertSync>? Alerts,
    IReadOnlyList<SafetyActuatorSync> Actuators,
    IReadOnlyList<SafetyActionSync> Actions);

public interface ISafetySyncService
{
    void RequirePlantKey(string? presentedKey);

    Task ApplyAsync(SafetySync sync, CancellationToken cancellationToken);
}
