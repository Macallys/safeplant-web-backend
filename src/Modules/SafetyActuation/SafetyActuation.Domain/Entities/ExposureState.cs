namespace SafetyActuation.Domain.Entities;

public sealed class ExposureState
{
    private ExposureState()
    {
    }

    public Guid AreaId { get; private set; }

    public string Severity { get; private set; } = "";

    public bool Highlighted { get; private set; }

    public static ExposureState Record(Guid areaId, string severity, bool highlighted) =>
        new()
        {
            AreaId = areaId,
            Severity = severity,
            Highlighted = highlighted,
        };

    public void Apply(string severity, bool highlighted)
    {
        Severity = severity;
        Highlighted = highlighted;
    }
}
