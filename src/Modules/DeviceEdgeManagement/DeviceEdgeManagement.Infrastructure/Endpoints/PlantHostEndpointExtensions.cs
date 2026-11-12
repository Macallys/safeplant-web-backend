using DeviceEdgeManagement.Infrastructure.Mqtt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DeviceEdgeManagement.Infrastructure.Endpoints;

public static class PlantHostEndpointExtensions
{
    public static IEndpointRouteBuilder MapPlantHostEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/v1/overrides", async (
            HttpContext http,
            PlantOverrideReceiver receiver,
            CancellationToken cancellationToken) =>
        {
            using var reader = new StreamReader(http.Request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);
            var plantKey = http.Request.Headers[DeviceEdgeManagement.Domain.Services.TelemetryHeaders.PlantKey].ToString();
            await receiver.AcceptAsync(plantKey, body, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
