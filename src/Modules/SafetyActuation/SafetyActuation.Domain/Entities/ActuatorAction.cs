namespace SafetyActuation.Domain.Entities;

public sealed class ActuatorAction
{
    private ActuatorAction()
    {
    }

    public Guid Id { get; private set; }

    public Guid AreaId { get; private set; }

    public string Type { get; private set; } = "";

    public bool Succeeded { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }
}
