using System.Globalization;
using System.Text.Json;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Interface.Assemblers;

public static class SafetySyncAssembler
{
    public static SafetySync Read(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new AppException("invalid_safety", 422);
        }

        var areaId = ReadGuid(body, "areaId");
        string? severity = null;
        var highlighted = false;
        IReadOnlyList<SafetyAlertSync>? alerts = null;
        if (body.TryGetProperty("exposure", out var exposure) && exposure.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            if (exposure.ValueKind != JsonValueKind.Object)
            {
                throw new AppException("invalid_safety", 422);
            }

            severity = ReadString(exposure, "severity");
            if (!exposure.TryGetProperty("highlighted", out var highlightedValue)
                || highlightedValue.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw new AppException("invalid_safety", 422);
            }

            highlighted = highlightedValue.GetBoolean();
            alerts = ReadAlerts(body);
        }

        return new SafetySync(areaId, severity, highlighted, alerts, ReadActuators(body), ReadActions(body));
    }

    private static IReadOnlyList<SafetyAlertSync> ReadAlerts(JsonElement body)
    {
        if (!body.TryGetProperty("alerts", out var alerts) || alerts.ValueKind != JsonValueKind.Array)
        {
            throw new AppException("invalid_safety", 422);
        }

        var items = new List<SafetyAlertSync>();
        foreach (var alert in alerts.EnumerateArray())
        {
            if (alert.ValueKind != JsonValueKind.Object)
            {
                throw new AppException("invalid_safety", 422);
            }

            items.Add(new SafetyAlertSync(
                ReadGuid(alert, "id"),
                ReadTimestamp(alert, "raisedAt"),
                ReadOptionalTimestamp(alert, "withdrawnAt")));
        }

        return items;
    }

    private static IReadOnlyList<SafetyActuatorSync> ReadActuators(JsonElement body)
    {
        if (!body.TryGetProperty("actuators", out var actuators) || actuators.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return [];
        }

        if (actuators.ValueKind != JsonValueKind.Array)
        {
            throw new AppException("invalid_safety", 422);
        }

        var items = new List<SafetyActuatorSync>();
        foreach (var actuator in actuators.EnumerateArray())
        {
            if (actuator.ValueKind != JsonValueKind.Object)
            {
                throw new AppException("invalid_safety", 422);
            }

            items.Add(new SafetyActuatorSync(
                ReadGuid(actuator, "id"),
                ReadString(actuator, "type"),
                ReadString(actuator, "runState"),
                ReadString(actuator, "mode"),
                ReadTimestamp(actuator, "lastChangedAt")));
        }

        return items;
    }

    private static IReadOnlyList<SafetyActionSync> ReadActions(JsonElement body)
    {
        if (!body.TryGetProperty("actions", out var actions) || actions.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return [];
        }

        if (actions.ValueKind != JsonValueKind.Array)
        {
            throw new AppException("invalid_safety", 422);
        }

        var items = new List<SafetyActionSync>();
        foreach (var action in actions.EnumerateArray())
        {
            if (action.ValueKind != JsonValueKind.Object
                || !action.TryGetProperty("succeeded", out var succeeded)
                || succeeded.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw new AppException("invalid_safety", 422);
            }

            items.Add(new SafetyActionSync(
                ReadGuid(action, "id"),
                ReadString(action, "type"),
                succeeded.GetBoolean(),
                ReadTimestamp(action, "recordedAt")));
        }

        return items;
    }

    private static string ReadString(JsonElement body, string name)
    {
        if (!body.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new AppException("invalid_safety", 422);
        }

        return property.GetString() ?? "";
    }

    private static Guid ReadGuid(JsonElement body, string name)
    {
        var text = ReadString(body, name);
        if (!Guid.TryParse(text, out var id))
        {
            throw new AppException("invalid_safety", 422);
        }

        return id;
    }

    private static DateTimeOffset ReadTimestamp(JsonElement body, string name)
    {
        var text = ReadString(body, name);
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
        {
            throw new AppException("invalid_safety", 422);
        }

        return value;
    }

    private static DateTimeOffset? ReadOptionalTimestamp(JsonElement body, string name)
    {
        if (!body.TryGetProperty(name, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String
            || !DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
        {
            throw new AppException("invalid_safety", 422);
        }

        return value;
    }
}
