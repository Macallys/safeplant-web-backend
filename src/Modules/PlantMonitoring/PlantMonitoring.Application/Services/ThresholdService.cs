using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace PlantMonitoring.Application.Services;

public sealed class ThresholdService : IThresholdService
{
    private readonly IAreaStore _areas;
    private readonly IThresholdStore _thresholds;
    private readonly IClock _clock;
    private readonly int _co2MinPpm;
    private readonly int _co2MaxPpm;
    private readonly int _noiseMinDb;
    private readonly int _noiseMaxDb;

    public ThresholdService(
        IAreaStore areas,
        IThresholdStore thresholds,
        IClock clock,
        int co2MinPpm,
        int co2MaxPpm,
        int noiseMinDb,
        int noiseMaxDb)
    {
        _areas = areas;
        _thresholds = thresholds;
        _clock = clock;
        _co2MinPpm = co2MinPpm;
        _co2MaxPpm = co2MaxPpm;
        _noiseMinDb = noiseMinDb;
        _noiseMaxDb = noiseMaxDb;
    }

    public async Task<AreaThresholds> ConfigureAsync(
        string role,
        string channel,
        string? areaId,
        decimal? co2LimitPpm,
        decimal? noiseLimitDb,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var co2 = RequireInteger(co2LimitPpm, _co2MinPpm, _co2MaxPpm, "co2_limit_out_of_range");
        var noise = RequireInteger(noiseLimitDb, _noiseMinDb, _noiseMaxDb, "noise_limit_out_of_range");
        var area = await FindAreaAsync(areaId, cancellationToken);

        var existing = await _thresholds.FindByAreaIdAsync(area.Id, cancellationToken);
        if (existing is null)
        {
            return await _thresholds.AddAsync(
                new AreaThresholds(area.Id, co2, noise, _clock.UtcNow),
                cancellationToken);
        }

        if (existing.Co2LimitPpm == co2 && existing.NoiseLimitDb == noise)
        {
            return existing;
        }

        existing.Replace(co2, noise, _clock.UtcNow);
        await _thresholds.SaveChangesAsync(cancellationToken);
        return existing;
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

    private static void RequireSupervisor(string role, string channel)
    {
        if (role != "Supervisor" || channel != "Mobile")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }

    private static int RequireInteger(decimal? value, int min, int max, string code)
    {
        if (value is null || value != decimal.Truncate(value.Value) || value < int.MinValue || value > int.MaxValue)
        {
            throw new AppException(code, 422);
        }

        var whole = (int)value.Value;
        if (whole < min || whole > max)
        {
            throw new AppException(code, 422);
        }

        return whole;
    }
}
