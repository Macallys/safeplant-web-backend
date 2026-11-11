namespace SafetyActuation.Domain.Services;

public sealed record AlertItem(Guid Id, Guid AreaId, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record ActionItem(Guid Id, Guid AreaId, string Type, bool Succeeded, DateTimeOffset RecordedAt);

public interface ISafetyReadService
{
    Task<IReadOnlyList<AlertItem>> ActiveAlertsAsync(
        string role,
        string channel,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AlertItem>> ExposuresAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ActionItem>> ActionsAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken);
}

public interface IOverrideService
{
    Task OverrideAsync(
        string role,
        string channel,
        string? areaId,
        string? actuatorType,
        string? runState,
        CancellationToken cancellationToken);
}
