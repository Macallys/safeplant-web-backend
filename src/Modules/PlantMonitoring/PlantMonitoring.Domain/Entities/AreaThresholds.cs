namespace PlantMonitoring.Domain.Entities;

public sealed class AreaThresholds
{
    private AreaThresholds()
    {
    }

    public AreaThresholds(Guid areaId, int co2LimitPpm, int noiseLimitDb, DateTimeOffset updatedAt)
    {
        AreaId = areaId;
        Co2LimitPpm = co2LimitPpm;
        NoiseLimitDb = noiseLimitDb;
        UpdatedAt = updatedAt;
    }

    public Guid AreaId { get; private set; }

    public int Co2LimitPpm { get; private set; }

    public int NoiseLimitDb { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Replace(int co2LimitPpm, int noiseLimitDb, DateTimeOffset updatedAt)
    {
        Co2LimitPpm = co2LimitPpm;
        NoiseLimitDb = noiseLimitDb;
        UpdatedAt = updatedAt;
    }
}
