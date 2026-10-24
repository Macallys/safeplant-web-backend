using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace PlantMonitoring.Application.Services;

public sealed class AreaService : IAreaService
{
    private const int NameMaxLength = 120;
    private const int LocationMaxLength = 200;

    private readonly IAreaStore _store;
    private readonly IClock _clock;

    public AreaService(IAreaStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public async Task<IndustrialArea> CreateAsync(
        string role,
        string channel,
        string? name,
        string? location,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var areaName = RequireText(name, NameMaxLength);
        var areaLocation = RequireText(location, LocationMaxLength);

        if (await _store.FindByNameAsync(areaName, cancellationToken) is not null)
        {
            throw new AppException("area_name_duplicate", 409);
        }

        var area = new IndustrialArea(Guid.NewGuid(), areaName, areaLocation, _clock.UtcNow);
        await _store.AddAsync(area, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
        return area;
    }

    public async Task<IndustrialArea> UpdateAsync(
        string role,
        string channel,
        string? areaId,
        string? name,
        string? location,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var areaName = RequireText(name, NameMaxLength);
        var areaLocation = RequireText(location, LocationMaxLength);
        var area = await FindAreaAsync(areaId, cancellationToken);

        var taken = await _store.FindByNameAsync(areaName, cancellationToken);
        if (taken is not null && taken.Id != area.Id)
        {
            throw new AppException("area_name_duplicate", 409);
        }

        if (area.Name == areaName && area.Location == areaLocation)
        {
            return area;
        }

        area.Update(areaName, areaLocation, _clock.UtcNow);
        await _store.SaveChangesAsync(cancellationToken);
        return area;
    }

    private async Task<IndustrialArea> FindAreaAsync(string? areaId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(areaId, out var id))
        {
            throw new AppException("area_not_found", 404);
        }

        var area = await _store.FindByIdAsync(id, cancellationToken);
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

    private static string RequireText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AppException("invalid_area", 422);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new AppException("invalid_area", 422);
        }

        return trimmed;
    }
}
