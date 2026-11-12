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

    public static EnvironmentalAlert Raise(Guid id, Guid areaId, DateTimeOffset raisedAt) =>
        new()
        {
            Id = id,
            AreaId = areaId,
            RaisedAt = raisedAt,
        };

    public void Withdraw(DateTimeOffset withdrawnAt)
    {
        WithdrawnAt ??= withdrawnAt;
    }
}
