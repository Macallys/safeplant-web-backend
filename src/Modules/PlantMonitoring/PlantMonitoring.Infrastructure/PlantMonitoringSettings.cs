namespace PlantMonitoring.Infrastructure;

public sealed class PlantMonitoringSettings
{
    public int Co2MinPpm { get; set; } = 1;

    public int Co2MaxPpm { get; set; } = 50000;

    public int NoiseMinDb { get; set; } = 1;

    public int NoiseMaxDb { get; set; } = 140;

    public int HistoryMaxDays { get; set; } = 31;
}
