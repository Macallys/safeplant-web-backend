using DeviceEdgeManagement.Domain.Services;
using DeviceEdgeManagement.Interface.Assemblers;
using IAM.Domain.Services;
using IAM.Interface.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DeviceEdgeManagement.Interface.Endpoints;

public static class DeviceEdgeEndpointExtensions
{
    public static IEndpointRouteBuilder MapDeviceEdgeEndpoints(this IEndpointRouteBuilder app)
    {
        var secured = app.MapGroup("").AddEndpointFilter<RequireSessionFilter>();

        secured.MapPost("/api/v1/devices", async (
            IssueDeviceBody body,
            HttpContext http,
            IDeviceCredentialService credentials,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var issued = await credentials.IssueAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                body.DeviceId,
                body.AreaId,
                body.Kind,
                cancellationToken);
            return Results.Created($"/api/v1/devices/{issued.DeviceId}", DeviceAssembler.ToIssued(issued));
        });

        secured.MapGet("/api/v1/devices", async (
            HttpContext http,
            IDeviceCredentialService credentials,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var items = await credentials.ListAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                cancellationToken);
            return Results.Ok(DeviceAssembler.ToList(items));
        });

        secured.MapPost("/api/v1/devices/{deviceId}/revoke", async (
            string deviceId,
            HttpContext http,
            IDeviceCredentialService credentials,
            CancellationToken cancellationToken) =>
        {
            var session = Current(http);
            var revoked = await credentials.RevokeAsync(
                session.Role.ToString(),
                session.Channel.ToString(),
                deviceId,
                cancellationToken);
            return Results.Ok(DeviceAssembler.ToRevoked(revoked));
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
