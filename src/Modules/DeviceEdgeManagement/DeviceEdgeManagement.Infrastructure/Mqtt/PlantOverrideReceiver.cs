using System.Text.Json;
using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class PlantOverrideReceiver
{
    private readonly PlantEdgeSettings _settings;
    private readonly IPlantLocalStore _store;
    private readonly IActuatorPublisher _publisher;
    private readonly IClock _clock;

    public PlantOverrideReceiver(
        PlantEdgeSettings settings,
        IPlantLocalStore store,
        IActuatorPublisher publisher,
        IClock clock)
    {
        _settings = settings;
        _store = store;
        _publisher = publisher;
        _clock = clock;
    }

    public async Task AcceptAsync(string? plantKey, string? body, CancellationToken cancellationToken)
    {
        if (!PlantKeys.Matches(_settings.PlantKey, plantKey))
        {
            throw new AppException("unauthorized", 401);
        }

        if (!TryRead(body, out var areaId, out var actuatorType, out var runState))
        {
            throw new AppException("invalid_action", 422);
        }

        var areas = await _store.ReadLoopAsync(cancellationToken);
        var area = areas.FirstOrDefault(item => item.AreaId == areaId);
        var now = _clock.UtcNow;
        var published = await _publisher.PublishAsync(
            areaId,
            actuatorType,
            runState,
            ActuatorPolicy.Overridden,
            cancellationToken);
        var current = area?.Actuators.FirstOrDefault(item => item.Type == actuatorType);
        var changes = new List<LocalActuator>();
        if (published)
        {
            changes.Add(new LocalActuator(
                current?.Id ?? Guid.NewGuid(),
                areaId,
                actuatorType,
                runState,
                ActuatorPolicy.Overridden,
                now));
        }

        var action = new LocalAction(Guid.NewGuid(), areaId, actuatorType, published, now, runState, false);
        var actuators = Merge(area?.Actuators ?? [], changes);
        var unsynced = (area?.Actions ?? []).Where(item => !item.Synced).Append(action).ToArray();
        var exposure = area?.Exposure;
        var alerts = area?.Alerts ?? [];
        await _store.ApplyUpdateAsync(
            new AreaUpdate(
                areaId,
                false,
                exposure?.Severity ?? ExposurePolicy.None,
                exposure?.Highlighted ?? false,
                false,
                alerts,
                changes,
                [action]),
            SafetySnapshot.Write(areaId, exposure, alerts, actuators, unsynced),
            cancellationToken);
    }

    private static bool TryRead(string? body, out Guid areaId, out string actuatorType, out string runState)
    {
        areaId = Guid.Empty;
        actuatorType = "";
        runState = "";
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("areaId", out var area)
                || area.ValueKind != JsonValueKind.String
                || !Guid.TryParse(area.GetString(), out areaId)
                || !root.TryGetProperty("actuatorType", out var type)
                || type.ValueKind != JsonValueKind.String
                || !root.TryGetProperty("runState", out var state)
                || state.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            actuatorType = type.GetString() ?? "";
            runState = state.GetString() ?? "";
            return ActuatorPolicy.IsValidPair(actuatorType, runState);
        }
        catch (JsonException)
        {
            return false;
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
}
