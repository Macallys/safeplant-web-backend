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
}
