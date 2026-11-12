namespace SafetyActuation.Domain.Services;

public static class ActuatorPolicy
{
    public const string Extractor = "Extractor";
    public const string Siren = "Siren";
    public const string Barrier = "Barrier";
    public const string Auto = "Auto";
    public const string Overridden = "Overridden";
    public const string On = "On";
    public const string Off = "Off";
    public const string Deployed = "Deployed";
    public const string Retracted = "Retracted";
    public const string Detected = "Detected";
    public const string Cleared = "Cleared";

    public readonly record struct Target(string Type, string RunState);

    public static IReadOnlyList<Target> Targets(
        bool co2Known,
        bool co2High,
        bool noiseKnown,
        bool noiseHigh,
        string? presence)
    {
        var targets = new List<Target>();
        if (co2Known)
        {
            targets.Add(new Target(Extractor, co2High ? On : Off));
        }

        if (noiseKnown)
        {
            targets.Add(new Target(Siren, noiseHigh ? On : Off));
        }

        if (presence is Detected or Cleared)
        {
            targets.Add(new Target(Barrier, presence == Cleared ? Deployed : Retracted));
        }

        return targets;
    }

    public static bool IsValidPair(string? actuatorType, string? runState) => (actuatorType, runState) switch
    {
        (Extractor, Off or On) => true,
        (Siren, Off or On) => true,
        (Barrier, Deployed or Retracted) => true,
        _ => false,
    };

    public static bool IsMode(string? mode) => mode is Auto or Overridden;
}
