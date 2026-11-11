using IAM.Domain.Services;
using IAM.Interface.Filters;
using SafetyActuation.Domain.Services;
using SafetyActuation.Interface.Assemblers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SafetyActuation.Interface.Endpoints;

public static class SafetyActuationEndpointExtensions
{
    public static IEndpointRouteBuilder MapSafetyActuationEndpoints(this IEndpointRouteBuilder app)
    {
        var secured = app.MapGroup("").AddEndpointFilter<RequireSessionFilter>();

        secured.MapGet("/api/v1/areas/operational-status", async (
            HttpContext http,
            IOperationalStatusService status,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var items = await status.ListAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                cancellationToken);
            return Results.Ok(OperationalStatusAssembler.ToList(items));
        });

        secured.MapGet("/api/v1/areas/{areaId}/operational-status", async (
            string areaId,
            HttpContext http,
            IOperationalStatusService status,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var item = await status.GetAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                cancellationToken);
            return Results.Ok(OperationalStatusAssembler.ToResponse(item));
        });

        secured.MapGet("/api/v1/alerts/active", async (
            HttpContext http,
            ISafetyReadService reads,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var alerts = await reads.ActiveAlertsAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                cancellationToken);
            return Results.Ok(SafetyRecordAssembler.ToAlerts(alerts));
        });

        secured.MapGet("/api/v1/areas/{areaId}/exposures", async (
            string areaId,
            HttpContext http,
            ISafetyReadService reads,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var alerts = await reads.ExposuresAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                cancellationToken);
            return Results.Ok(SafetyRecordAssembler.ToAlerts(alerts));
        });

        secured.MapGet("/api/v1/areas/{areaId}/actuator-actions", async (
            string areaId,
            HttpContext http,
            ISafetyReadService reads,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var actions = await reads.ActionsAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                cancellationToken);
            return Results.Ok(SafetyRecordAssembler.ToActions(actions));
        });

        secured.MapPost("/api/v1/areas/{areaId}/actuators/override", async (
            string areaId,
            OverrideBody body,
            HttpContext http,
            IOverrideService overrides,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            await overrides.OverrideAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                areaId,
                body.ActuatorType,
                body.RunState,
                cancellationToken);
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
