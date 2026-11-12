using DeviceEdgeManagement.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class OutboundQueueWorker : BackgroundService
{
    private readonly IPlantLocalStore _store;
    private readonly CloudIngestClient _cloud;
    private readonly TelemetryForwarder _forwarder;
    private readonly ILogger<OutboundQueueWorker> _logger;

    public OutboundQueueWorker(
        IPlantLocalStore store,
        CloudIngestClient cloud,
        TelemetryForwarder forwarder,
        ILogger<OutboundQueueWorker> logger)
    {
        _store = store;
        _cloud = cloud;
        _forwarder = forwarder;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Outbound queue failed.");
            }

            try
            {
                await DrainSafetyAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Safety queue failed.");
            }

            await PlantEdgeTiming.DelayAsync(stoppingToken);
        }
    }

    private async Task DrainSafetyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var pending = await _store.PeekSafetyAsync(cancellationToken);
            if (pending is null)
            {
                return;
            }

            var result = await _cloud.PutSafetyAsync(pending.Body, cancellationToken);
            if (result.Delivery == CloudDelivery.Unavailable)
            {
                _logger.LogWarning(
                    "Queued safety stays in the queue with status {StatusCode} and code {Code}.",
                    result.StatusCode,
                    result.Code ?? "");
                return;
            }

            var synced = result.Delivery == CloudDelivery.Stored
                ? SafetySnapshot.ActionIds(pending.Body)
                : [];
            await _store.CompleteSafetyAsync(pending.Id, pending.Body, synced, cancellationToken);
            if (result.Delivery == CloudDelivery.Rejected)
            {
                _logger.LogWarning(
                    "Queued safety was discarded with status {StatusCode} and code {Code}.",
                    result.StatusCode,
                    result.Code ?? "");
            }
        }
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var pending = await _store.PeekTelemetryAsync(cancellationToken);
            if (pending is null)
            {
                return;
            }

            var result = await _cloud.PostTelemetryAsync(pending.Body, cancellationToken);
            if (result.Delivery == CloudDelivery.Unavailable)
            {
                _logger.LogWarning(
                    "Queued telemetry stays in the queue with status {StatusCode} and code {Code}.",
                    result.StatusCode,
                    result.Code ?? "");
                return;
            }

            await _store.DropAsync(pending.Id, cancellationToken);
            if (result.Delivery == CloudDelivery.Stored)
            {
                await _forwarder.RememberStoredAsync(pending.Body, cancellationToken);
                _logger.LogInformation("Queued telemetry was stored.");
                continue;
            }

            _logger.LogWarning(
                "Queued telemetry was discarded with status {StatusCode} and code {Code}.",
                result.StatusCode,
                result.Code ?? "");
        }
    }
}
