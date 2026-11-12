using System.Globalization;
using System.Text.Json;
using DeviceEdgeManagement.Domain.Services;
using SharedKernel.Application;

namespace DeviceEdgeManagement.Interface.Assemblers;

public static class TelemetryAssembler
{
    public static TelemetryIngest Read(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new AppException("invalid_reading", 422);
        }

        return new TelemetryIngest(
            ReadString(body, "deviceId"),
            ReadString(body, "secret"),
            ReadString(body, "measure"),
            ReadInt(body, "ppm"),
            ReadInt(body, "db"),
            ReadString(body, "state"),
            ReadTimestamp(body, "recordedAt"));
    }

    private static string? ReadString(JsonElement body, string name)
    {
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new AppException("invalid_reading", 422);
        }

        return property.GetString();
    }

    private static int? ReadInt(JsonElement body, string name)
    {
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var value))
        {
            throw new AppException("invalid_reading", 422);
        }

        return value;
    }

    private static DateTimeOffset? ReadTimestamp(JsonElement body, string name)
    {
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String
            || !DateTimeOffset.TryParse(
                property.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var recordedAt))
        {
            throw new AppException("invalid_reading", 422);
        }

        return recordedAt;
    }
}
