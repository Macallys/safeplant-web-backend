using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Application.Services;

public sealed class OverrideService : IOverrideService
{
    private readonly IAreaStore _areas;

    public OverrideService(IAreaStore areas)
    {
        _areas = areas;
    }

    public async Task OverrideAsync(
        string role,
        string channel,
        string? areaId,
        string? actuatorType,
        string? runState,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        if (!IsValidPair(actuatorType, runState))
        {
            throw new AppException("invalid_action", 400);
        }

        await FindAreaAsync(areaId, cancellationToken);
        throw new AppException("edge_unreachable", 503);
    }

    private async Task FindAreaAsync(string? areaId, CancellationToken cancellationToken)
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
    }

    private static bool IsValidPair(string? actuatorType, string? runState) => (actuatorType, runState) switch
    {
        ("Extractor", "Off" or "On") => true,
        ("Siren", "Off" or "On") => true,
        ("Barrier", "Deployed" or "Retracted") => true,
        _ => false
    };

    private static void RequireSupervisor(string role, string channel)
    {
        if (role != "Supervisor" || channel != "Mobile")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }
}
