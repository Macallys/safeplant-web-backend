namespace PlantMonitoring.Domain.Entities;

public sealed class NoiseReading
{
    private NoiseReading()
    {
    }

    public Guid Id { get; private set; }

    public string DeviceId { get; private set; } = "";

    public Guid AreaId { get; private set; }

    public int Db { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    public static NoiseReading Record(
        Guid id,
        string deviceId,
        Guid areaId,
        int decibels,
        DateTimeOffset recordedAt) =>
        new()
        {
            Id = id,
            DeviceId = deviceId,
            AreaId = areaId,
            Db = decibels,
            RecordedAt = recordedAt,
        };
}
