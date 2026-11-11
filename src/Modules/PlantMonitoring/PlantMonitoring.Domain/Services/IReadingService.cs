namespace PlantMonitoring.Domain.Services;

public interface IReadingService
{
    Task<LatestReadings> LatestAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DashboardItem>> DashboardAsync(
        string role,
        string channel,
        CancellationToken cancellationToken);

    Task<AreaHistory> HistoryAsync(
        string role,
        string channel,
        string? areaId,
        string? from,
        string? to,
        CancellationToken cancellationToken);
}
