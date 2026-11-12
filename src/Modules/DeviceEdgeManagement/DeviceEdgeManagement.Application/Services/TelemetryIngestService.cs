using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Application.Services;

public sealed class TelemetryIngestService : ITelemetryIngestService
{
    private const int DeviceIdMaxLength = 64;
    private const string CarbonDioxide = "carbonDioxide";
    private const string Noise = "noise";
    private const string Presence = "presence";
    private const string SensorKind = "Sensor";
    private const string PresenceDetected = "Detected";
    private const string PresenceCleared = "Cleared";

    private readonly string _plantKey;
    private readonly IDeviceCredentialStore _credentials;
    private readonly IDeviceStore _devices;
    private readonly IReadingStore _readings;
    private readonly ISecretHasher _secrets;
    private readonly IClock _clock;

    public TelemetryIngestService(
        string plantKey,
        IDeviceCredentialStore credentials,
        IDeviceStore devices,
        IReadingStore readings,
        ISecretHasher secrets,
        IClock clock)
    {
        _plantKey = plantKey;
        _credentials = credentials;
        _devices = devices;
        _readings = readings;
        _secrets = secrets;
        _clock = clock;
    }

    public void RequirePlantKey(string? presentedKey)
    {
        if (!FixedEquals(_plantKey, presentedKey))
        {
            throw new AppException("unauthorized", 401);
        }
    }

    public async Task AcceptAsync(TelemetryIngest reading, CancellationToken cancellationToken)
    {
        var deviceId = RequireDeviceId(reading.DeviceId);
        var secret = RequireSecret(reading.Secret);
        var measure = RequireMeasure(reading.Measure);
        var credential = await _credentials.FindByDeviceIdAsync(deviceId, cancellationToken);
        if (credential is null)
        {
            throw new AppException("device_not_found", 404);
        }

        if (!FixedEquals(_secrets.Hash(secret), credential.SecretHash))
        {
            throw new AppException("unauthorized", 401);
        }

        if (credential.RevokedAt is not null)
        {
            throw new AppException("credential_revoked", 403);
        }

        if (credential.ExpiresAt <= _clock.UtcNow)
        {
            throw new AppException("credential_expired", 403);
        }

        var device = await _devices.FindByDeviceIdAsync(deviceId, cancellationToken);
        if (device is null || device.Kind != SensorKind)
        {
            throw new AppException("device_not_assigned", 422);
        }

        var recordedAt = reading.RecordedAt ?? _clock.UtcNow;
        var id = Guid.NewGuid();
        switch (measure)
        {
            case CarbonDioxide:
                var ppm = RequireNonNegative(reading.Ppm);
                await _readings.AppendCarbonDioxideAsync(
                    CarbonDioxideReading.Record(id, deviceId, device.AreaId, ppm, recordedAt),
                    cancellationToken);
                return;
            case Noise:
                var decibels = RequireNonNegative(reading.Decibels);
                await _readings.AppendNoiseAsync(
                    NoiseReading.Record(id, deviceId, device.AreaId, decibels, recordedAt),
                    cancellationToken);
                return;
            case Presence:
                var state = RequirePresence(reading.State);
                await _readings.AppendPresenceAsync(
                    PresenceChange.Record(id, deviceId, device.AreaId, state, recordedAt),
                    cancellationToken);
                return;
            default:
                throw new AppException("invalid_reading", 422);
        }
    }

    private static string RequireDeviceId(string? deviceId)
    {
        var parsed = deviceId?.Trim() ?? "";
        if (parsed.Length is < 1 or > DeviceIdMaxLength)
        {
            throw new AppException("invalid_reading", 422);
        }

        return parsed;
    }

    private static string RequireMeasure(string? measure)
    {
        if (measure is not (CarbonDioxide or Noise or Presence))
        {
            throw new AppException("invalid_reading", 422);
        }

        return measure;
    }

    private static string RequireSecret(string? secret)
    {
        var parsed = secret?.Trim() ?? "";
        if (parsed.Length == 0)
        {
            throw new AppException("invalid_reading", 422);
        }

        return parsed;
    }

    private static int RequireNonNegative(int? value)
    {
        if (value is null or < 0)
        {
            throw new AppException("invalid_reading", 422);
        }

        return value.Value;
    }

    private static string RequirePresence(string? state)
    {
        if (state is not (PresenceDetected or PresenceCleared))
        {
            throw new AppException("invalid_reading", 422);
        }

        return state;
    }

    private static bool FixedEquals(string expected, string? presented)
    {
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
        var presentedBytes = System.Text.Encoding.UTF8.GetBytes(presented);
        if (expectedBytes.Length != presentedBytes.Length)
        {
            return false;
        }

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(expectedBytes, presentedBytes);
    }
}
