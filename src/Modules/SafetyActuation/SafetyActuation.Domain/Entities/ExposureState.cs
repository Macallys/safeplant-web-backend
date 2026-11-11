namespace SafetyActuation.Domain.Entities;

public sealed class ExposureState
{
    private ExposureState()
    {
    }

    public Guid AreaId { get; private set; }

    public string Severity { get; private set; } = "";

    public bool Highlighted { get; private set; }
}
