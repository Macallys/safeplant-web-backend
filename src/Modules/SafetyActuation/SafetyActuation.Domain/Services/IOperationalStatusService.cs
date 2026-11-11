namespace SafetyActuation.Domain.Services;

public sealed record ActuatorPoint(string Type, string RunState, string Mode, DateTimeOffset LastChangedAt);

public sealed record OperationalStatus(
    Guid AreaId,
    string Name,
    int? Co2Ppm,
    int? NoiseDb,
    string? PresenceState,
    string Severity,
    bool AlertActive,
    bool Highlighted,
    IReadOnlyList<ActuatorPoint> Actuators);

public interface IOperationalStatusService
{
    Task<IReadOnlyList<OperationalStatus>> ListAsync(
        string role,
        string channel,
        CancellationToken cancellationToken);

    Task<OperationalStatus> GetAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken);
}
