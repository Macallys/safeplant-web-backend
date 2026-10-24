using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Interface.Assemblers;

public sealed record ThresholdBody(decimal? Co2LimitPpm, decimal? NoiseLimitDb);

public sealed record ThresholdResponse(string AreaId, int Co2LimitPpm, int NoiseLimitDb);

public static class ThresholdAssembler
{
    public static ThresholdResponse ToResponse(AreaThresholds thresholds) =>
        new(thresholds.AreaId.ToString(), thresholds.Co2LimitPpm, thresholds.NoiseLimitDb);
}
