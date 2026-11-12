using System.Security.Cryptography;
using System.Text;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Application.Services;

public sealed class SafetySyncService : ISafetySyncService
{
    private readonly string _plantKey;
    private readonly IAreaStore _areas;
    private readonly ISafetySyncStore _sync;

    public SafetySyncService(string plantKey, IAreaStore areas, ISafetySyncStore sync)
    {
        _plantKey = plantKey;
        _areas = areas;
        _sync = sync;
    }

    public void RequirePlantKey(string? presentedKey)
    {
        if (!Matches(_plantKey, presentedKey))
        {
            throw new AppException("unauthorized", 401);
        }
    }

    public async Task ApplyAsync(SafetySync sync, CancellationToken cancellationToken)
    {
        if (sync.Severity is not null)
        {
            if (sync.Severity is not (ExposurePolicy.None or ExposurePolicy.Medium or ExposurePolicy.High)
                || sync.Highlighted != ExposurePolicy.Highlighted(sync.Severity)
                || sync.Alerts is null)
            {
                throw new AppException("invalid_safety", 422);
            }

            var open = sync.Alerts.Count(alert => alert.WithdrawnAt is null);
            var expectedOpen = sync.Severity == ExposurePolicy.None ? 0 : 1;
            if (open != expectedOpen || sync.Alerts.Select(alert => alert.Id).Distinct().Count() != sync.Alerts.Count)
            {
                throw new AppException("invalid_safety", 422);
            }
        }
        else if (sync.Alerts is not null)
        {
            throw new AppException("invalid_safety", 422);
        }

        if (sync.Actuators.Select(actuator => actuator.Type).Distinct().Count() != sync.Actuators.Count
            || sync.Actuators.Select(actuator => actuator.Id).Distinct().Count() != sync.Actuators.Count
            || sync.Actions.Select(action => action.Id).Distinct().Count() != sync.Actions.Count)
        {
            throw new AppException("invalid_safety", 422);
        }

        foreach (var actuator in sync.Actuators)
        {
            if (!ActuatorPolicy.IsValidPair(actuator.Type, actuator.RunState) || !ActuatorPolicy.IsMode(actuator.Mode))
            {
                throw new AppException("invalid_safety", 422);
            }
        }

        foreach (var action in sync.Actions)
        {
            if (action.Type is not (ActuatorPolicy.Extractor or ActuatorPolicy.Siren or ActuatorPolicy.Barrier))
            {
                throw new AppException("invalid_safety", 422);
            }
        }

        var area = await _areas.FindByIdAsync(sync.AreaId, cancellationToken);
        if (area is null)
        {
            throw new AppException("invalid_safety", 422);
        }

        await _sync.UpsertAsync(sync, cancellationToken);
    }

    private static bool Matches(string expected, string? presented)
    {
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var presentedBytes = Encoding.UTF8.GetBytes(presented);
        if (expectedBytes.Length != presentedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, presentedBytes);
    }
}
