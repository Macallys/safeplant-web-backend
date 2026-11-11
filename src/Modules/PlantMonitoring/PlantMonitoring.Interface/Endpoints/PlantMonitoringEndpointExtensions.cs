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

        secured.MapPost("/api/v1/areas/{areaId}/devices", async (
            string areaId,
            DeviceBody body,
            HttpContext http,
            IDeviceService devices,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var device = await devices.AssociateAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                body.DeviceId,
                body.Kind,
                cancellationToken);
            return Results.Created(
                $"/api/v1/areas/{device.AreaId}/devices/{device.Id}",
                DeviceAssembler.ToResponse(device));
        });

        secured.MapGet("/api/v1/areas/{areaId}/setup", async (
            string areaId,
            HttpContext http,
            IDeviceService devices,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var sheet = await devices.GetSetupAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                cancellationToken);
            return Results.Ok(DeviceAssembler.ToSetup(sheet));
        });

        secured.MapGet("/api/v1/areas/{areaId}/telemetry/latest", async (
            string areaId,
            HttpContext http,
            IReadingService readings,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var latest = await readings.LatestAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                cancellationToken);
            return Results.Ok(ReadingAssembler.ToLatest(latest));
        });

        secured.MapGet("/api/v1/metrics/dashboard", async (
            HttpContext http,
            IReadingService readings,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var items = await readings.DashboardAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                cancellationToken);
            return Results.Ok(ReadingAssembler.ToDashboard(items));
        });

        secured.MapGet("/api/v1/areas/{areaId}/history", async (
            string areaId,
            string? from,
            string? to,
            HttpContext http,
            IReadingService readings,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var history = await readings.HistoryAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                from,
                to,
                cancellationToken);
            return Results.Ok(ReadingAssembler.ToHistory(history));
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
