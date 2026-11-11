namespace DeviceEdgeManagement.Domain.Entities;

public sealed class DeviceCredential
{
    private DeviceCredential()
    {
    }

    public DeviceCredential(string deviceId, string secretHash, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        DeviceId = deviceId;
        SecretHash = secretHash;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public string DeviceId { get; private set; } = "";

    public string SecretHash { get; private set; } = "";

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public void Revoke(DateTimeOffset revokedAt)
    {
        if (RevokedAt is null)
        {
            RevokedAt = revokedAt;
        }
    }
}
