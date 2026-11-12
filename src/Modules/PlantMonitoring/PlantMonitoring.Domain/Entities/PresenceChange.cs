namespace PlantMonitoring.Domain.Entities;

public sealed class PresenceChange
{
    private PresenceChange()
    {
    }

    public Guid Id { get; private set; }

    public string DeviceId { get; private set; } = "";

    public Guid AreaId { get; private set; }

    public string State { get; private set; } = "";

    public DateTimeOffset RecordedAt { get; private set; }

    public static PresenceChange Record(
        Guid id,
        string deviceId,
        Guid areaId,
        string state,
        DateTimeOffset recordedAt) =>
        new()
        {
            Id = id,
            DeviceId = deviceId,
            AreaId = areaId,
            State = state,
            RecordedAt = recordedAt,
        };
}
