using PlantMonitoring.Domain.Entities;

namespace PlantMonitoring.Interface.Assemblers;

public sealed record AreaBody(string? Name, string? Location);

public sealed record AreaResponse(string Id, string Name, string Location);

public static class AreaAssembler
{
    public static AreaResponse ToResponse(IndustrialArea area) =>
        new(area.Id.ToString(), area.Name, area.Location);
}
