namespace DeviceEdgeManagement.Domain.Services;

public sealed record IssuedCredential(
    string DeviceId,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    string Secret);

public sealed record RevokedCredential(string DeviceId, DateTimeOffset RevokedAt);

public sealed record DeviceListItem(
    string DeviceId,
    Guid? AreaId,
    string? Kind,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt);

public interface IDeviceCredentialService
{
    Task<IssuedCredential> IssueAsync(
        string role,
        string channel,
        string? deviceId,
        string? areaId,
        string? kind,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DeviceListItem>> ListAsync(
        string role,
        string channel,
        CancellationToken cancellationToken);

    Task<RevokedCredential> RevokeAsync(
        string role,
        string channel,
        string? deviceId,
        CancellationToken cancellationToken);
}
