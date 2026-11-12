using System.Text.Json;
using DeviceEdgeManagement.Domain.Services;
using DeviceEdgeManagement.Interface.Assemblers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Application;

namespace DeviceEdgeManagement.Interface.Endpoints;

public static class TelemetryIngestEndpointExtensions
{
    public static IEndpointRouteBuilder MapTelemetryIngestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/v1/telemetry", async (
            HttpContext http,
            ITelemetryIngestService ingest,
            CancellationToken cancellationToken) =>
        {
            var plantKey = http.Request.Headers[TelemetryHeaders.PlantKey].ToString();
            ingest.RequirePlantKey(plantKey);

            JsonDocument document;
            try
            {
                document = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: cancellationToken);
            }
            catch (JsonException)
            {
                throw new AppException("invalid_reading", 422);
            }

            using (document)
            {
                var reading = TelemetryAssembler.Read(document.RootElement);
                await ingest.AcceptAsync(reading, cancellationToken);
            }

            return Results.NoContent();
        });

        return app;
    }
}
