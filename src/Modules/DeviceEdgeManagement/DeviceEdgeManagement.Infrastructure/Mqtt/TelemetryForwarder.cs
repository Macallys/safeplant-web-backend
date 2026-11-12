using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

public sealed class TelemetryForwarder
{
    private readonly IPlantLocalStore _store;
    private readonly CloudIngestClient _cloud;
    private readonly ISecretHasher _hasher;
    private readonly IClock _clock;
    private readonly ILogger<TelemetryForwarder> _logger;

    public TelemetryForwarder(
        IPlantLocalStore store,
        CloudIngestClient cloud,
        ISecretHasher hasher,
        IClock clock,
        ILogger<TelemetryForwarder> logger)
    {
        _store = store;
        _cloud = cloud;
        _hasher = hasher;
        _clock = clock;
        _logger = logger;
    }

    public async Task ForwardAsync(string deviceId, string? payload, CancellationToken cancellationToken)
    {
        if (!TelemetryAdmission.TryParse(payload, deviceId, out var message))
        {
            _logger.LogWarning("MQTT telemetry from {DeviceId} is not a reading.", deviceId);
            return;
        }

        var now = _clock.UtcNow;
        if (message.RecordedAt is null)
        {
            message = TelemetryAdmission.Stamp(message, now);
        }

        var credential = await _store.FindCredentialAsync(message.DeviceId, cancellationToken);
        var device = credential is null
            ? null
            : await _store.FindDeviceAsync(message.DeviceId, cancellationToken);
        if (!TelemetryAdmission.TryAdmit(message, credential, device, _hasher, now, out var reading))
        {
            _logger.LogWarning("Telemetry from {DeviceId} was discarded locally.", deviceId);
            return;
        }

        var result = await _cloud.PostTelemetryAsync(message.Body, cancellationToken);
        if (result.Delivery == CloudDelivery.Stored)
        {
            await RememberAsync(message, reading, cancellationToken);
            _logger.LogInformation("Telemetry from {DeviceId} was stored.", deviceId);
            return;
        }

        if (result.Delivery == CloudDelivery.Rejected)
        {
            _logger.LogWarning(
                "Telemetry from {DeviceId} was discarded with status {StatusCode} and code {Code}.",
                deviceId,
                result.StatusCode,
                result.Code ?? "");
            return;
        }

        await _store.EnqueueTelemetryAsync(reading, message.Body, cancellationToken);
        _logger.LogWarning("Telemetry from {DeviceId} was queued.", deviceId);
    }

    public async Task RememberStoredAsync(string body, CancellationToken cancellationToken)
    {
        if (!TelemetryAdmission.TryParse(body, null, out var message))
        {
            return;
        }

        await RememberAsync(message, null, cancellationToken);
    }

    private async Task RememberAsync(
        TelemetryMessage message,
        LocalReading? reading,
        CancellationToken cancellationToken)
    {
        reading ??= await ResolveReadingAsync(message, cancellationToken);
        if (reading is null)
        {
            return;
        }

        await _store.SaveReadingAsync(reading, cancellationToken);
    }

    private async Task<LocalReading?> ResolveReadingAsync(TelemetryMessage message, CancellationToken cancellationToken)
    {
        var device = await _store.FindDeviceAsync(message.DeviceId, cancellationToken);
        if (device is null || device.Kind != TelemetryAdmission.SensorKind)
        {
            var sheet = await _cloud.GetSheetAsync(cancellationToken);
            if (sheet is not null)
            {
                await _store.ReplaceSheetAsync(sheet, cancellationToken);
            }

            device = await _store.FindDeviceAsync(message.DeviceId, cancellationToken);
        }

        if (device is null || device.Kind != TelemetryAdmission.SensorKind)
        {
            return null;
        }

        return TelemetryAdmission.ToReading(message, device.AreaId, _clock.UtcNow);
    }
}
