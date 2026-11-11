namespace SafetyActuation.Domain.Entities;

public sealed class EnvironmentalAlert
{
    private EnvironmentalAlert()
    {
    }

    public Guid Id { get; private set; }

    public Guid AreaId { get; private set; }

    public DateTimeOffset RaisedAt { get; private set; }

    public DateTimeOffset? WithdrawnAt { get; private set; }
}
