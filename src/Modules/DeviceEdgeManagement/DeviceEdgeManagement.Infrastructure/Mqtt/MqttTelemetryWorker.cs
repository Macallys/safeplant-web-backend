using DeviceEdgeManagement.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class MqttTelemetryWorker : BackgroundService
{
    private const string TopicFilter = "safeplant/v1/+/telemetry";

    private readonly PlantEdgeSettings _settings;
    private readonly TelemetryForwarder _forwarder;
    private readonly ILogger<MqttTelemetryWorker> _logger;

    public MqttTelemetryWorker(
        PlantEdgeSettings settings,
        TelemetryForwarder forwarder,
        ILogger<MqttTelemetryWorker> logger)
    {
        _settings = settings;
        _forwarder = forwarder;
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

                await Task.Delay(PlantEdgeTiming.Interval, stoppingToken);
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
                await PlantEdgeTiming.DelayAsync(stoppingToken);
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
            await _forwarder.ForwardAsync(deviceId, payload, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Telemetry from {DeviceId} was not handled.", deviceId);
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
}
