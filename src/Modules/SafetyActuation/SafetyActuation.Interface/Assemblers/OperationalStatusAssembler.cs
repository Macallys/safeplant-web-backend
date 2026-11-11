using SafetyActuation.Domain.Services;

namespace SafetyActuation.Interface.Assemblers;

public sealed record ActuatorStatusResponse(string Type, string RunState, string Mode, DateTimeOffset LastChangedAt);

public sealed record OperationalStatusResponse(
    string AreaId,
    string Name,
    int? Co2Ppm,
    int? NoiseDb,
    string? PresenceState,
    string Severity,
    bool AlertActive,
    bool Highlighted,
    IReadOnlyList<ActuatorStatusResponse> Actuators);

public sealed record OperationalStatusListResponse(IReadOnlyList<OperationalStatusResponse> Items);

public static class OperationalStatusAssembler
{
    public static OperationalStatusResponse ToResponse(OperationalStatus status) =>
        new(
            status.AreaId.ToString(),
            status.Name,
            status.Co2Ppm,
            status.NoiseDb,
            status.PresenceState,
            status.Severity,
            status.AlertActive,
            status.Highlighted,
            status.Actuators
                .Select(actuator => new ActuatorStatusResponse(
                    actuator.Type,
                    actuator.RunState,
                    actuator.Mode,
                    actuator.LastChangedAt))
                .ToArray());

    public static OperationalStatusListResponse ToList(IReadOnlyList<OperationalStatus> items) =>
        new(items.Select(ToResponse).ToArray());
}
