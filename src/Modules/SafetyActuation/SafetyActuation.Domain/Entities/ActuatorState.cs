namespace SafetyActuation.Domain.Entities;

public sealed class ActuatorState
{
    private ActuatorState()
    {
    }

    public Guid Id { get; private set; }

    public Guid AreaId { get; private set; }

    public string Type { get; private set; } = "";

    public string RunState { get; private set; } = "";

    public string Mode { get; private set; } = "";

    public DateTimeOffset LastChangedAt { get; private set; }

    public static ActuatorState Record(
        Guid id,
        Guid areaId,
        string type,
        string runState,
        string mode,
        DateTimeOffset lastChangedAt) =>
        new()
        {
            Id = id,
            AreaId = areaId,
            Type = type,
            RunState = runState,
            Mode = mode,
            LastChangedAt = lastChangedAt,
        };

    public void Apply(string runState, string mode, DateTimeOffset lastChangedAt)
    {
        RunState = runState;
        Mode = mode;
        LastChangedAt = lastChangedAt;
    }
}
