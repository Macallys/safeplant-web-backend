using System.Globalization;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Application;

namespace PlantMonitoring.Application.Services;

public sealed class ReadingService : IReadingService
{
    private readonly IAreaStore _areas;
    private readonly IReadingStore _readings;
    private readonly IThresholdStore _thresholds;
    private readonly IAreaSafetyStatus _safety;
    private readonly int _historyMaxDays;

    public ReadingService(
        IAreaStore areas,
        IReadingStore readings,
        IThresholdStore thresholds,
        IAreaSafetyStatus safety,
        int historyMaxDays)
    {
        _areas = areas;
        _readings = readings;
        _thresholds = thresholds;
        _safety = safety;
        _historyMaxDays = historyMaxDays;
    }

    public async Task<LatestReadings> LatestAsync(
        string role,
        string channel,
        string? areaId,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var area = await FindAreaAsync(areaId, cancellationToken);
        var carbonDioxide = await _readings.LatestCarbonDioxideAsync(area.Id, cancellationToken);
        var noise = await _readings.LatestNoiseAsync(area.Id, cancellationToken);
        var presence = await _readings.LatestPresenceAsync(area.Id, cancellationToken);

        return new LatestReadings(
            area.Id,
            carbonDioxide is null
                ? null
                : new CarbonDioxidePoint(carbonDioxide.DeviceId, carbonDioxide.Ppm, carbonDioxide.RecordedAt),
            noise is null ? null : new NoisePoint(noise.DeviceId, noise.Db, noise.RecordedAt),
            presence is null ? null : new PresencePoint(presence.DeviceId, presence.State, presence.RecordedAt));
    }

    public async Task<IReadOnlyList<DashboardItem>> DashboardAsync(
        string role,
        string channel,
        CancellationToken cancellationToken)
    {
        RequirePlantManager(role, channel);
        var areas = await _areas.ListAsync(cancellationToken);
        var limits = await _thresholds.ListAsync(cancellationToken);
        var limitByArea = limits.ToDictionary(entry => entry.AreaId);
        var items = new List<DashboardItem>(areas.Count);
        foreach (var area in areas)
        {
            var carbonDioxide = await _readings.LatestCarbonDioxideAsync(area.Id, cancellationToken);
            var noise = await _readings.LatestNoiseAsync(area.Id, cancellationToken);
            var presence = await _readings.LatestPresenceAsync(area.Id, cancellationToken);
            var safety = _safety.ForArea(area.Id);
            limitByArea.TryGetValue(area.Id, out var limit);
            items.Add(new DashboardItem(
                area.Id,
                area.Name,
                carbonDioxide?.Ppm,
                noise?.Db,
                presence?.State,
                safety.Severity,
                safety.AlertActive,
                safety.Highlighted,
                limit?.Co2LimitPpm,
                limit?.NoiseLimitDb));
        }

        return items;
    }

    public async Task<AreaHistory> HistoryAsync(
        string role,
        string channel,
        string? areaId,
        string? from,
        string? to,
        CancellationToken cancellationToken)
    {
        RequirePlantManager(role, channel);
        var (periodFrom, periodTo) = RequirePeriod(from, to);
        var area = await FindAreaAsync(areaId, cancellationToken);
        var carbonDioxide = await _readings.CarbonDioxideBetweenAsync(area.Id, periodFrom, periodTo, cancellationToken);
        var noise = await _readings.NoiseBetweenAsync(area.Id, periodFrom, periodTo, cancellationToken);
        var presence = await _readings.PresenceBetweenAsync(area.Id, periodFrom, periodTo, cancellationToken);

        return new AreaHistory(
            area.Id,
            carbonDioxide
                .Select(reading => new CarbonDioxideHistoryItem(reading.Id, reading.DeviceId, reading.Ppm, reading.RecordedAt))
                .ToArray(),
            noise.Select(reading => new NoiseHistoryItem(reading.Id, reading.DeviceId, reading.Db, reading.RecordedAt)).ToArray(),
            presence
                .Select(reading => new PresenceHistoryItem(reading.Id, reading.DeviceId, reading.State, reading.RecordedAt))
                .ToArray(),
            _safety.Alerts(area.Id, periodFrom, periodTo),
            _safety.Actions(area.Id, periodFrom, periodTo));
    }

    private async Task<IndustrialArea> FindAreaAsync(string? areaId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(areaId, out var id))
        {
            throw new AppException("area_not_found", 404);
        }

        var area = await _areas.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new AppException("area_not_found", 404);
        }

        return area;
    }

    private (DateTimeOffset From, DateTimeOffset To) RequirePeriod(string? from, string? to)
    {
        if (!DateTimeOffset.TryParse(from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedFrom)
            || !DateTimeOffset.TryParse(to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedTo)
            || parsedFrom >= parsedTo)
        {
            throw new AppException("invalid_period", 422);
        }

        TimeSpan span;
        try
        {
            span = parsedTo - parsedFrom;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new AppException("invalid_period", 422);
        }

        if (span > TimeSpan.FromDays(_historyMaxDays))
        {
            throw new AppException("invalid_period", 422);
        }

        return (parsedFrom, parsedTo);
    }

    private static void RequireSupervisor(string role, string channel)
    {
        if (role != "Supervisor" || channel != "Mobile")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }

    private static void RequirePlantManager(string role, string channel)
    {
        if (role != "PlantManager" || channel != "Web")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }
}
