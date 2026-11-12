namespace DeviceEdgeManagement.Domain.Services;

public sealed record LocalExposure(Guid AreaId, string Severity, bool Highlighted);

public sealed record LocalAlert(Guid Id, Guid AreaId, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record LocalActuator(
    Guid Id,
    Guid AreaId,
    string Type,
    string RunState,
    string Mode,
    DateTimeOffset LastChangedAt);

public sealed record LocalAction(
    Guid Id,
    Guid AreaId,
    string Type,
    bool Succeeded,
    DateTimeOffset RecordedAt,
    string AttemptedRunState,
    bool Synced);

public sealed record AreaUpdate(
    Guid AreaId,
    bool WriteExposure,
    string Severity,
    bool Highlighted,
    bool WriteAlerts,
    IReadOnlyList<LocalAlert> Alerts,
    IReadOnlyList<LocalActuator> Actuators,
    IReadOnlyList<LocalAction> Actions);

public sealed record AreaLoopState(
    Guid AreaId,
    int? Co2LimitPpm,
    int? NoiseLimitDb,
    LocalReading? CarbonDioxide,
    LocalReading? Noise,
    LocalReading? Presence,
    LocalExposure? Exposure,
    IReadOnlyList<LocalAlert> Alerts,
    IReadOnlyList<LocalActuator> Actuators,
    IReadOnlyList<LocalAction> Actions);
