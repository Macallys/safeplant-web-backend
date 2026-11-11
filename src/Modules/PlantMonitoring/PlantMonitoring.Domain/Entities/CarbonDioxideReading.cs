namespace PlantMonitoring.Domain.Entities;

public sealed class CarbonDioxideReading
{
    private CarbonDioxideReading()
    {
    }

    public Guid Id { get; private set; }

    public string DeviceId { get; private set; } = "";

    public Guid AreaId { get; private set; }

    public int Ppm { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }
}
