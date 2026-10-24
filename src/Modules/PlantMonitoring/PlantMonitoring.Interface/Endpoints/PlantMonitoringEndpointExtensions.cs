using IAM.Domain.Services;
using IAM.Interface.Filters;
using PlantMonitoring.Domain.Services;
using PlantMonitoring.Interface.Assemblers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PlantMonitoring.Interface.Endpoints;

public static class PlantMonitoringEndpointExtensions
{
    public static IEndpointRouteBuilder MapPlantMonitoringEndpoints(this IEndpointRouteBuilder app)
    {
        var secured = app.MapGroup("").AddEndpointFilter<RequireSessionFilter>();

        secured.MapPost("/api/v1/areas", async (
            AreaBody body,
            HttpContext http,
            IAreaService areas,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var area = await areas.CreateAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                body.Name,
                body.Location,
                cancellationToken);
            return Results.Created($"/api/v1/areas/{area.Id}", AreaAssembler.ToResponse(area));
        });

        secured.MapPut("/api/v1/areas/{areaId}", async (
            string areaId,
            AreaBody body,
            HttpContext http,
            IAreaService areas,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var area = await areas.UpdateAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                body.Name,
                body.Location,
                cancellationToken);
            return Results.Ok(AreaAssembler.ToResponse(area));
        });

        secured.MapPut("/api/v1/areas/{areaId}/thresholds", async (
            string areaId,
            ThresholdBody body,
            HttpContext http,
            IThresholdService thresholds,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var result = await thresholds.ConfigureAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                body.Co2LimitPpm,
                body.NoiseLimitDb,
                cancellationToken);
            return Results.Ok(ThresholdAssembler.ToResponse(result));
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
