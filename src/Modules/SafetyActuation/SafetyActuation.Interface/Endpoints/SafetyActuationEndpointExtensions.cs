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

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
