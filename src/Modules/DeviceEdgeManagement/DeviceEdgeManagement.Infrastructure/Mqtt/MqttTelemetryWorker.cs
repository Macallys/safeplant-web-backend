using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DeviceEdgeManagement.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class MqttTelemetryWorker : BackgroundService
{
    public const string HttpClientName = "telemetry-ingest";
    private const string TopicFilter = "safeplant/v1/+/telemetry";

    private readonly PlantEdgeSettings _settings;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<MqttTelemetryWorker> _logger;

    public MqttTelemetryWorker(
        PlantEdgeSettings settings,
        IHttpClientFactory http,
        ILogger<MqttTelemetryWorker> logger)
    {
        _settings = settings;
        _http = http;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        using var client = factory.CreateMqttClient();
        client.ApplicationMessageReceivedAsync += message => OnMessageAsync(message, stoppingToken);

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.MqttHost, _settings.MqttPort)
            .WithClientId("plant-host")
            .Build();
        var subscription = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(filter => filter.WithTopic(TopicFilter))
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!client.IsConnected)
                {
                    await client.ConnectAsync(options, stoppingToken);
                    await client.SubscribeAsync(subscription, stoppingToken);
                    _logger.LogInformation("MQTT subscribed to {Topic}.", TopicFilter);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "MQTT broker is not reachable at {Host}:{Port}.",
                    _settings.MqttHost,
                    _settings.MqttPort);
                await DelayAsync(stoppingToken);
            }
        }

        if (client.IsConnected)
        {
            await client.DisconnectAsync(cancellationToken: stoppingToken);
        }
    }

    private async Task OnMessageAsync(MqttApplicationMessageReceivedEventArgs message, CancellationToken stoppingToken)
    {
        var topic = message.ApplicationMessage.Topic;
        if (!TryReadDeviceId(topic, out var deviceId))
        {
            _logger.LogWarning("MQTT topic {Topic} is not a telemetry reading.", topic);
            return;
        }

        try
        {
            var payload = message.ApplicationMessage.ConvertPayloadToString();
            if (!TryWithDeviceId(payload, deviceId, out var body))
            {
                _logger.LogWarning("MQTT telemetry from {DeviceId} is not a JSON object.", deviceId);
                return;
            }

            var http = _http.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_settings.IngestUrl, "/internal/v1/telemetry"));
            request.Headers.TryAddWithoutValidation(TelemetryHeaders.PlantKey, _settings.PlantKey);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await http.SendAsync(request, stoppingToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogInformation("Telemetry from {DeviceId} was stored.", deviceId);
                return;
            }

            var code = await ReadErrorCodeAsync(response, stoppingToken);
            _logger.LogWarning(
                "Telemetry from {DeviceId} was discarded with status {StatusCode} and code {Code}.",
                deviceId,
                (int)response.StatusCode,
                code ?? "");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Telemetry from {DeviceId} was discarded.", deviceId);
        }
    }

    private static bool TryReadDeviceId(string? topic, out string deviceId)
    {
        deviceId = "";
        var parts = topic?.Split('/') ?? [];
        if (parts.Length != 4
            || parts[0] != "safeplant"
            || parts[1] != "v1"
            || parts[3] != "telemetry")
        {
            return false;
        }

        var parsed = parts[2].Trim();
        if (parsed.Length is < 1 or > 64 || parsed.Contains('+') || parsed.Contains('#'))
        {
            return false;
        }

        deviceId = parsed;
        return true;
    }

    private static bool TryWithDeviceId(string? payload, string deviceId, out string body)
    {
        body = "";
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        try
        {
            if (JsonNode.Parse(payload) is not JsonObject document)
            {
                return false;
            }

            document["deviceId"] = deviceId;
            body = document.ToJsonString();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
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

    private static async Task DelayAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
