using DeviceEdgeManagement.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class PlantSheetPullWorker : BackgroundService
{
    private readonly CloudIngestClient _cloud;
    private readonly IPlantLocalStore _store;
    private readonly ILogger<PlantSheetPullWorker> _logger;

    public PlantSheetPullWorker(
        CloudIngestClient cloud,
        IPlantLocalStore store,
        ILogger<PlantSheetPullWorker> logger)
    {
        _cloud = cloud;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sheet = await _cloud.GetSheetAsync(stoppingToken);
                if (sheet is not null)
                {
                    await _store.ReplaceSheetAsync(sheet, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Plant sheet was not refreshed.");
            }

            await PlantEdgeTiming.DelayAsync(stoppingToken);
        }
    }
}
