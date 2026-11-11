using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Domain.Services;

public sealed record AreaSetupSheet(
    Guid Id,
    string Name,
    string Location,
    int? Co2LimitPpm,
    int? NoiseLimitDb,
    IReadOnlyList<AreaDevice> Devices);
