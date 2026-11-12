using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Application.Services;

public sealed class OverrideService : IOverrideService
{
    private readonly IAreaStore _areas;
    private readonly IPlantOverrideLink _plant;

    public OverrideService(IAreaStore areas, IPlantOverrideLink plant)
    {
        _areas = areas;
        _plant = plant;
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

        var area = await FindAreaAsync(areaId, cancellationToken);
        await _plant.SendAsync(area.Id, actuatorType!, runState!, cancellationToken);
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
