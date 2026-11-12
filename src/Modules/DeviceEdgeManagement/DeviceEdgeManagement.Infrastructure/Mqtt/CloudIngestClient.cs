using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DeviceEdgeManagement.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public enum CloudDelivery
{
    Stored,
    Rejected,
    Unavailable,
}

public sealed record CloudDeliveryResult(CloudDelivery Delivery, int? StatusCode, string? Code);

public sealed class CloudIngestClient
{
    public const string HttpClientName = "telemetry-ingest";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly PlantEdgeSettings _settings;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<CloudIngestClient> _logger;

    public CloudIngestClient(
        PlantEdgeSettings settings,
        IHttpClientFactory http,
        ILogger<CloudIngestClient> logger)
    {
        _settings = settings;
        _http = http;
        _logger = logger;
    }

    public Task<CloudDeliveryResult> PostTelemetryAsync(string body, CancellationToken cancellationToken) =>
        DeliverAsync(HttpMethod.Post, "/internal/v1/telemetry", body, cancellationToken);

    private async Task<CloudDeliveryResult> DeliverAsync(
        HttpMethod method,
        string path,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendAsync(method, path, body, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new CloudDeliveryResult(CloudDelivery.Stored, 204, null);
            }

            var code = await ReadErrorCodeAsync(response, cancellationToken);
            var status = (int)response.StatusCode;
            var delivery = status is >= 400 and < 500 ? CloudDelivery.Rejected : CloudDelivery.Unavailable;
            return new CloudDeliveryResult(delivery, status, code);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Cloud ingest is not reachable.");
            return new CloudDeliveryResult(CloudDelivery.Unavailable, null, null);
        }
    }

    public Task<CloudDeliveryResult> PutSafetyAsync(string body, CancellationToken cancellationToken) =>
        DeliverAsync(HttpMethod.Put, "/internal/v1/safety", body, cancellationToken);

    public async Task<PlantSheet?> GetSheetAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendAsync(HttpMethod.Get, "/internal/v1/plant-sheet", null, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning("Plant sheet was not refreshed. Status {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<SheetDto>(JsonOptions, cancellationToken);
            return dto is null ? null : Map(dto);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Plant sheet was not refreshed.");
            return null;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        string? body,
        CancellationToken cancellationToken)
    {
        var http = _http.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(method, new Uri(_settings.IngestUrl, path));
        request.Headers.TryAddWithoutValidation(TelemetryHeaders.PlantKey, _settings.PlantKey);
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return await http.SendAsync(request, cancellationToken);
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
            {
                return code.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static PlantSheet Map(SheetDto dto) =>
        new(
            dto.Areas.Select(area => new PlantAreaReplica(area.Id, area.Name, area.Location, area.UpdatedAt)).ToArray(),
            dto.Thresholds.Select(threshold => new PlantThresholdReplica(
                threshold.AreaId,
                threshold.Co2LimitPpm,
                threshold.NoiseLimitDb,
                threshold.UpdatedAt)).ToArray(),
            dto.Devices.Select(device => new PlantDeviceReplica(device.DeviceId, device.AreaId, device.Kind)).ToArray(),
            dto.Credentials.Select(credential => new PlantCredentialReplica(
                credential.DeviceId,
                credential.SecretHash,
                credential.ExpiresAt,
                credential.RevokedAt)).ToArray());

    private sealed class SheetDto
    {
        public List<AreaDto> Areas { get; set; } = [];

        public List<ThresholdDto> Thresholds { get; set; } = [];

        public List<DeviceDto> Devices { get; set; } = [];

        public List<CredentialDto> Credentials { get; set; } = [];
    }

    private sealed class AreaDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public string Location { get; set; } = "";

        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class ThresholdDto
    {
        public Guid AreaId { get; set; }

        public int Co2LimitPpm { get; set; }

        public int NoiseLimitDb { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class DeviceDto
    {
        public string DeviceId { get; set; } = "";

        public Guid AreaId { get; set; }

        public string Kind { get; set; } = "";
    }

    private sealed class CredentialDto
    {
        public string DeviceId { get; set; } = "";

        public string SecretHash { get; set; } = "";

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }
    }
}
