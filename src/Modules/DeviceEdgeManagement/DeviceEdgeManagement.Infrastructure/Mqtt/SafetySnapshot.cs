using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DeviceEdgeManagement.Domain.Services;

namespace DeviceEdgeManagement.Infrastructure.Mqtt;

internal static class SafetySnapshot
{
    public static string Write(
        Guid areaId,
        LocalExposure? exposure,
        IReadOnlyList<LocalAlert> alerts,
        IReadOnlyList<LocalActuator> actuators,
        IReadOnlyList<LocalAction> unsyncedActions)
    {
        var document = new JsonObject
        {
            ["areaId"] = areaId.ToString(),
        };
        if (exposure is not null)
        {
            document["exposure"] = new JsonObject
            {
                ["severity"] = exposure.Severity,
                ["highlighted"] = exposure.Highlighted,
            };
            document["alerts"] = Alerts(alerts);
        }

        if (actuators.Count > 0)
        {
            document["actuators"] = Actuators(actuators);
        }

        if (unsyncedActions.Count > 0)
        {
            document["actions"] = Actions(unsyncedActions);
        }

        return document.ToJsonString();
    }

    public static IReadOnlyList<Guid> ActionIds(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("actions", out var actions) || actions.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var ids = new List<Guid>();
            foreach (var action in actions.EnumerateArray())
            {
                if (action.TryGetProperty("id", out var id)
                    && id.ValueKind == JsonValueKind.String
                    && Guid.TryParse(id.GetString(), out var parsed))
                {
                    ids.Add(parsed);
                }
            }

            return ids;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static JsonArray Alerts(IReadOnlyList<LocalAlert> alerts)
    {
        var items = new JsonArray();
        foreach (var alert in alerts)
        {
            var item = new JsonObject
            {
                ["id"] = alert.Id.ToString(),
                ["raisedAt"] = alert.RaisedAt.ToString("O", CultureInfo.InvariantCulture),
            };
            if (alert.WithdrawnAt is not null)
            {
                item["withdrawnAt"] = alert.WithdrawnAt.Value.ToString("O", CultureInfo.InvariantCulture);
            }

            items.Add(item);
        }

        return items;
    }

    private static JsonArray Actuators(IReadOnlyList<LocalActuator> actuators)
    {
        var items = new JsonArray();
        foreach (var actuator in actuators)
        {
            items.Add(new JsonObject
            {
                ["id"] = actuator.Id.ToString(),
                ["type"] = actuator.Type,
                ["runState"] = actuator.RunState,
                ["mode"] = actuator.Mode,
                ["lastChangedAt"] = actuator.LastChangedAt.ToString("O", CultureInfo.InvariantCulture),
            });
        }

        return items;
    }

    private static JsonArray Actions(IReadOnlyList<LocalAction> actions)
    {
        var items = new JsonArray();
        foreach (var action in actions)
        {
            items.Add(new JsonObject
            {
                ["id"] = action.Id.ToString(),
                ["type"] = action.Type,
                ["succeeded"] = action.Succeeded,
                ["recordedAt"] = action.RecordedAt.ToString("O", CultureInfo.InvariantCulture),
            });
        }

        return items;
    }
}
