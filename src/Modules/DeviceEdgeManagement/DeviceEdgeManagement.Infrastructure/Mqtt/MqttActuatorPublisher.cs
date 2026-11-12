using System.Text;
using System.Text.Json.Nodes;
using DeviceEdgeManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class MqttActuatorPublisher : IActuatorPublisher
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttActuatorPublisher> _logger;

    public MqttActuatorPublisher(PlantEdgeSettings settings, ILogger<MqttActuatorPublisher> logger)
    {
        _logger = logger;
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(settings.MqttHost, settings.MqttPort)
            .WithClientId("plant-host-actuators")
            .Build();
    }

    public async Task<bool> PublishAsync(
        Guid areaId,
        string type,
        string runState,
        string mode,
        CancellationToken cancellationToken)
    {
        var payload = new JsonObject
        {
            ["areaId"] = areaId.ToString(),
            ["runState"] = runState,
            ["mode"] = mode,
        }.ToJsonString();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(_options, cancellationToken);
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"safeplant/v1/actuators/{type}")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            var result = await _client.PublishAsync(message, cancellationToken);
            if (result.IsSuccess)
            {
                return true;
            }

            _logger.LogWarning("Actuator {Type} was not published. Reason {Reason}.", type, result.ReasonCode);
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Actuator {Type} was not published.", type);
            return false;
        }
        finally
        {
            _gate.Release();
        }
    }
}
