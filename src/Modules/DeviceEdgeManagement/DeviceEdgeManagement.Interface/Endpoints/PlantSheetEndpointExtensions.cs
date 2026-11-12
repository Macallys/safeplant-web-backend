using DeviceEdgeManagement.Domain.Services;
using DeviceEdgeManagement.Interface.Assemblers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DeviceEdgeManagement.Interface.Endpoints;

public static class PlantSheetEndpointExtensions
{
    public static IEndpointRouteBuilder MapPlantSheetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/internal/v1/plant-sheet", async (
            HttpContext http,
            IPlantSheetService sheet,
            CancellationToken cancellationToken) =>
        {
            sheet.RequirePlantKey(http.Request.Headers[TelemetryHeaders.PlantKey].ToString());
            var snapshot = await sheet.ReadAsync(cancellationToken);
            return Results.Ok(PlantSheetAssembler.ToResponse(snapshot));
        });

        return app;
    }
}
