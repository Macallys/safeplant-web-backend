using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DeviceEdgeManagement.Domain.Interfaces;

namespace DeviceEdgeManagement.Domain.Services;

public sealed record TelemetryMessage(
    string DeviceId,
    string Secret,
    string Measure,
    int? Ppm,
    int? Decibels,
    string? State,
    DateTimeOffset? RecordedAt,
    string Body);

public static class TelemetryAdmission
{
    public const string CarbonDioxide = "carbonDioxide";
    public const string Noise = "noise";
    public const string Presence = "presence";
    public const string SensorKind = "Sensor";
    private const int DeviceIdMaxLength = 64;
    private const string PresenceDetected = "Detected";
    private const string PresenceCleared = "Cleared";

    public static bool TryParse(string? payload, string? deviceId, out TelemetryMessage message)
    {
        message = null!;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var body = document.RootElement;
            var parsedDeviceId = deviceId?.Trim() ?? "";
            if (parsedDeviceId.Length == 0 && !TryString(body, "deviceId", out parsedDeviceId))
            {
                return false;
            }

            parsedDeviceId = parsedDeviceId.Trim();
            if (parsedDeviceId.Length is < 1 or > DeviceIdMaxLength)
            {
                return false;
            }

            if (!TryString(body, "secret", out var secret))
            {
                return false;
            }

            secret = secret.Trim();
            if (secret.Length == 0)
            {
                return false;
            }

            if (!TryString(body, "measure", out var measure) || measure is not (CarbonDioxide or Noise or Presence))
            {
                return false;
            }

            if (!TryOptionalInt(body, "ppm", out var ppm) || !TryOptionalInt(body, "db", out var decibels))
            {
                return false;
            }

            if (!TryOptionalString(body, "state", out var state) || !TryOptionalTimestamp(body, out var recordedAt))
            {
                return false;
            }

            if (!ValueMatches(measure, ppm, decibels, state))
            {
                return false;
            }

            message = new TelemetryMessage(
                parsedDeviceId,
                secret,
                measure,
                ppm,
                decibels,
                state,
                recordedAt,
                WriteBody(parsedDeviceId, secret, measure, ppm, decibels, state, recordedAt));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static TelemetryMessage Stamp(TelemetryMessage message, DateTimeOffset recordedAt) =>
        message with
        {
            RecordedAt = recordedAt,
            Body = WriteBody(
                message.DeviceId,
                message.Secret,
                message.Measure,
                message.Ppm,
                message.Decibels,
                message.State,
                recordedAt),
        };

    public static bool TryAdmit(
        TelemetryMessage message,
        PlantCredentialReplica? credential,
        PlantDeviceReplica? device,
        ISecretHasher hasher,
        DateTimeOffset recordedAt,
        out LocalReading? reading)
    {
        reading = null;
        if (credential is null)
        {
            return true;
        }

        if (!PlantKeys.Matches(credential.SecretHash, hasher.Hash(message.Secret))
            || credential.RevokedAt is not null
            || credential.ExpiresAt <= recordedAt
            || device is null
            || device.Kind != SensorKind)
        {
            return false;
        }

        reading = ToReading(message, device.AreaId, recordedAt);
        return true;
    }

    public static LocalReading ToReading(TelemetryMessage message, Guid areaId, DateTimeOffset recordedAt) =>
        new(
            areaId,
            message.Measure,
            message.DeviceId,
            message.Ppm,
            message.Decibels,
            message.State,
            message.RecordedAt ?? recordedAt);

    private static bool ValueMatches(string measure, int? ppm, int? decibels, string? state) => measure switch
    {
        CarbonDioxide => ppm is >= 0,
        Noise => decibels is >= 0,
        Presence => state is PresenceDetected or PresenceCleared,
        _ => false,
    };

    private static string WriteBody(
        string deviceId,
        string secret,
        string measure,
        int? ppm,
        int? decibels,
        string? state,
        DateTimeOffset? recordedAt)
    {
        var document = new JsonObject
        {
            ["deviceId"] = deviceId,
            ["secret"] = secret,
            ["measure"] = measure,
        };
        if (ppm is not null)
        {
            document["ppm"] = ppm.Value;
        }

        if (decibels is not null)
        {
            document["db"] = decibels.Value;
        }

        if (state is not null)
        {
            document["state"] = state;
        }

        if (recordedAt is not null)
        {
            document["recordedAt"] = recordedAt.Value.ToString("O", CultureInfo.InvariantCulture);
        }

        return document.ToJsonString();
    }

    private static bool TryString(JsonElement body, string name, out string value)
    {
        value = "";
        if (!body.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? "";
        return true;
    }

    private static bool TryOptionalString(JsonElement body, string name, out string? value)
    {
        value = null;
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return true;
    }

    private static bool TryOptionalInt(JsonElement body, string name, out int? value)
    {
        value = null;
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var parsed))
        {
            return false;
        }

        value = parsed;
        return true;
    }

    private static bool TryOptionalTimestamp(JsonElement body, out DateTimeOffset? recordedAt)
    {
        recordedAt = null;
        if (!body.TryGetProperty("recordedAt", out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String
            || !DateTimeOffset.TryParse(
                property.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            return false;
        }

        recordedAt = parsed;
        return true;
    }
}
