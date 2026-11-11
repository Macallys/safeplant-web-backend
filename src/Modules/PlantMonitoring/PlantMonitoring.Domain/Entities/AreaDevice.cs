namespace PlantMonitoring.Domain.Entities;

public sealed class AreaDevice
{
    private AreaDevice()
    {
    }

    public AreaDevice(Guid id, Guid areaId, string deviceId, string kind)
    {
        Id = id;
        AreaId = areaId;
        DeviceId = deviceId;
        Kind = kind;
    }

    public Guid Id { get; private set; }

    public Guid AreaId { get; private set; }

    public string DeviceId { get; private set; } = "";

    public string Kind { get; private set; } = "";
}
