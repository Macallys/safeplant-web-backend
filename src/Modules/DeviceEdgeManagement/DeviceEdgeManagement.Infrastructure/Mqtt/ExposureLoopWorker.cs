using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SafetyActuation.Domain.Services;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class ExposureLoopWorker : BackgroundService
{
    private readonly IPlantLocalStore _store;
    private readonly IActuatorPublisher _publisher;
    private readonly IClock _clock;
    private readonly ILogger<ExposureLoopWorker> _logger;

    public ExposureLoopWorker(
        IPlantLocalStore store,
        IActuatorPublisher publisher,
        IClock clock,
        ILogger<ExposureLoopWorker> logger)
    {
        _store = store;
        _publisher = publisher;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PassAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Exposure loop failed.");
            }

            await PlantEdgeTiming.DelayAsync(stoppingToken);
        }
    }

    private async Task PassAsync(CancellationToken cancellationToken)
    {
        var areas = await _store.ReadLoopAsync(cancellationToken);
        var now = _clock.UtcNow;
        foreach (var area in areas)
        {
            if (area.CarbonDioxide is null && area.Noise is null && area.Presence is null)
            {
                continue;
            }

            var hasGas = area.CarbonDioxide is not null || area.Noise is not null;
            var severity = area.Exposure?.Severity ?? ExposurePolicy.None;
            var highlighted = area.Exposure?.Highlighted ?? false;
            var alerts = area.Alerts;
            var writeExposure = false;
            var writeAlerts = false;
            if (hasGas)
            {
                severity = ExposurePolicy.Severity(
                    area.CarbonDioxide?.Ppm,
                    area.Co2LimitPpm,
                    area.Noise?.Decibels,
                    area.NoiseLimitDb);
                highlighted = ExposurePolicy.Highlighted(severity);
                var nextAlerts = NextAlerts(area, severity, now);
                writeExposure = area.Exposure is null
                    || area.Exposure.Severity != severity
                    || area.Exposure.Highlighted != highlighted;
                writeAlerts = writeExposure || !SameAlerts(area.Alerts, nextAlerts);
                alerts = nextAlerts;
            }

            var actuatorChanges = new List<LocalActuator>();
            var newActions = new List<LocalAction>();
            var co2Ppm = area.CarbonDioxide?.Ppm;
            var noiseDb = area.Noise?.Decibels;
            var co2Known = co2Ppm is not null && area.Co2LimitPpm is not null;
            var noiseKnown = noiseDb is not null && area.NoiseLimitDb is not null;
            foreach (var target in ActuatorPolicy.Targets(
                co2Known,
                co2Known && co2Ppm!.Value >= area.Co2LimitPpm!.Value,
                noiseKnown,
                noiseKnown && noiseDb!.Value >= area.NoiseLimitDb!.Value,
                area.Presence?.State))
            {
                var current = area.Actuators.FirstOrDefault(item => item.Type == target.Type);
                if (current?.Mode == ActuatorPolicy.Overridden)
                {
                    continue;
                }

                if (current is not null && current.Mode == ActuatorPolicy.Auto && current.RunState == target.RunState)
                {
                    continue;
                }

                var published = await _publisher.PublishAsync(
                    area.AreaId,
                    target.Type,
                    target.RunState,
                    ActuatorPolicy.Auto,
                    cancellationToken);
                if (published)
                {
                    actuatorChanges.Add(new LocalActuator(
                        current?.Id ?? Guid.NewGuid(),
                        area.AreaId,
                        target.Type,
                        target.RunState,
                        ActuatorPolicy.Auto,
                        now));
                }

                var last = area.Actions
                    .Where(item => item.Type == target.Type)
                    .OrderByDescending(item => item.RecordedAt)
                    .FirstOrDefault();
                var repeatFailure = !published
                    && last is { Succeeded: false }
                    && last.AttemptedRunState == target.RunState;
                if (!repeatFailure)
                {
                    newActions.Add(new LocalAction(
                        Guid.NewGuid(),
                        area.AreaId,
                        target.Type,
                        published,
                        now,
                        target.RunState,
                        false));
                }
            }

            if (!writeExposure && !writeAlerts && actuatorChanges.Count == 0 && newActions.Count == 0)
            {
                continue;
            }

            var exposure = hasGas || area.Exposure is not null
                ? new LocalExposure(area.AreaId, severity, highlighted)
                : null;
            var mergedActuators = Merge(area.Actuators, actuatorChanges);
            var unsynced = area.Actions.Where(item => !item.Synced).Concat(newActions).ToArray();
            var body = SafetySnapshot.Write(area.AreaId, exposure, alerts, mergedActuators, unsynced);
            await _store.ApplyUpdateAsync(
                new AreaUpdate(
                    area.AreaId,
                    writeExposure,
                    severity,
                    highlighted,
                    writeAlerts,
                    alerts,
                    actuatorChanges,
                    newActions),
                body,
                cancellationToken);
            if (writeExposure)
            {
                _logger.LogInformation("Exposure for {AreaId} is {Severity}.", area.AreaId, severity);
            }
        }
    }

    private static IReadOnlyList<LocalActuator> Merge(
        IReadOnlyList<LocalActuator> current,
        IReadOnlyList<LocalActuator> changes)
    {
        var merged = current.ToList();
        foreach (var change in changes)
        {
            var index = merged.FindIndex(item => item.Type == change.Type);
            if (index >= 0)
            {
                merged[index] = change;
            }
            else
            {
                merged.Add(change);
            }
        }

        return merged;
    }

    private static IReadOnlyList<LocalAlert> NextAlerts(AreaLoopState area, string severity, DateTimeOffset now)
    {
        var alerts = area.Alerts.ToList();
        var open = alerts.Where(alert => alert.WithdrawnAt is null).OrderBy(alert => alert.RaisedAt).ToArray();
        if (severity == ExposurePolicy.None)
        {
            foreach (var alert in open)
            {
                Replace(alerts, alert with { WithdrawnAt = now });
            }

            return alerts;
        }

        if (open.Length == 0)
        {
            alerts.Add(new LocalAlert(Guid.NewGuid(), area.AreaId, now, null));
            return alerts;
        }

        for (var index = 1; index < open.Length; index++)
        {
            Replace(alerts, open[index] with { WithdrawnAt = now });
        }

        return alerts;
    }

    private static void Replace(List<LocalAlert> alerts, LocalAlert alert)
    {
        var index = alerts.FindIndex(item => item.Id == alert.Id);
        if (index >= 0)
        {
            alerts[index] = alert;
        }
    }

    private static bool SameAlerts(IReadOnlyList<LocalAlert> current, IReadOnlyList<LocalAlert> next)
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        foreach (var alert in next)
        {
            var existing = current.FirstOrDefault(item => item.Id == alert.Id);
            if (existing is null || existing.RaisedAt != alert.RaisedAt || existing.WithdrawnAt != alert.WithdrawnAt)
            {
                return false;
            }
        }

        return true;
    }
}
