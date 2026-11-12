using DeviceEdgeManagement.Domain.Services;

namespace DeviceEdgeManagement.Interface.Assemblers;

public sealed record IssueDeviceBody(string? DeviceId, string? AreaId, string? Kind);

public sealed record IssueDeviceResponse(
    string DeviceId,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    string Secret);

public sealed record DeviceListItemResponse(
    string DeviceId,
    string? AreaId,
    string? Kind,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset? LastSeenAt);

public sealed record DeviceListResponse(IReadOnlyList<DeviceListItemResponse> Items);

public sealed record RevokeDeviceResponse(string DeviceId, DateTimeOffset RevokedAt);

public static class DeviceAssembler
{
    public static IssueDeviceResponse ToIssued(IssuedCredential issued) =>
        new(issued.DeviceId, issued.IssuedAt, issued.ExpiresAt, issued.Secret);

    public static DeviceListResponse ToList(IReadOnlyList<DeviceListItem> items) =>
        new(items.Select(item => new DeviceListItemResponse(
            item.DeviceId,
            item.AreaId?.ToString(),
            item.Kind,
            item.IssuedAt,
            item.ExpiresAt,
            item.RevokedAt,
            item.LastSeenAt)).ToArray());

    public static RevokeDeviceResponse ToRevoked(RevokedCredential revoked) =>
        new(revoked.DeviceId, revoked.RevokedAt);
}
