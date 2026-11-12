using System.Text.Json;
using SafetyActuation.Domain.Services;
using SafetyActuation.Interface.Assemblers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Application;

namespace SafetyActuation.Interface.Endpoints;

public static class SafetySyncEndpointExtensions
{
    public const string PlantKeyHeader = "X-Plant-Key";

    public static IEndpointRouteBuilder MapSafetySyncEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/internal/v1/safety", async (
            HttpContext http,
            ISafetySyncService sync,
            CancellationToken cancellationToken) =>
        {
            sync.RequirePlantKey(http.Request.Headers[PlantKeyHeader].ToString());

            JsonDocument document;
            try
            {
                document = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: cancellationToken);
            }
            catch (JsonException)
            {
                throw new AppException("invalid_safety", 422);
            }

            using (document)
            {
                var snapshot = SafetySyncAssembler.Read(document.RootElement);
                await sync.ApplyAsync(snapshot, cancellationToken);
            }

            return Results.NoContent();
        });

        return app;
    }
}
