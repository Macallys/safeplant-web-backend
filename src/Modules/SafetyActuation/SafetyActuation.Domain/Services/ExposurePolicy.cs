namespace SafetyActuation.Domain.Services;

public static class ExposurePolicy
{
    public const string None = "None";
    public const string Medium = "Medium";
    public const string High = "High";

    public static string Severity(int? ppm, int? co2LimitPpm, int? decibels, int? noiseLimitDb)
    {
        var co2High = ppm is not null && co2LimitPpm is not null && ppm.Value >= co2LimitPpm.Value;
        var noiseHigh = decibels is not null && noiseLimitDb is not null && decibels.Value >= noiseLimitDb.Value;
        if (co2High && noiseHigh)
        {
            return High;
        }

        if (co2High || noiseHigh)
        {
            return Medium;
        }

        return None;
    }

    public static bool Highlighted(string severity) => severity == High;
}
