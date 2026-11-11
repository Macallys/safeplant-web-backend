using SafetyActuation.Domain.Services;

namespace SafetyActuation.Interface.Assemblers;

public sealed record AlertResponse(string Id, string AreaId, DateTimeOffset RaisedAt, DateTimeOffset? WithdrawnAt);

public sealed record AlertListResponse(IReadOnlyList<AlertResponse> Items);

public sealed record ActionResponse(string Id, string AreaId, string Type, bool Succeeded, DateTimeOffset RecordedAt);

public sealed record ActionListResponse(IReadOnlyList<ActionResponse> Items);

public sealed record OverrideBody(string? ActuatorType, string? RunState);

public static class SafetyRecordAssembler
{
    public static AlertListResponse ToAlerts(IReadOnlyList<AlertItem> alerts) =>
        new(alerts.Select(alert => new AlertResponse(
            alert.Id.ToString(),
            alert.AreaId.ToString(),
            alert.RaisedAt,
            alert.WithdrawnAt)).ToArray());

    public static ActionListResponse ToActions(IReadOnlyList<ActionItem> actions) =>
        new(actions.Select(action => new ActionResponse(
            action.Id.ToString(),
            action.AreaId.ToString(),
            action.Type,
            action.Succeeded,
            action.RecordedAt)).ToArray());
}
