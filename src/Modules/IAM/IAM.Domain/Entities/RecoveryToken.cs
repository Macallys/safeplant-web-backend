namespace IAM.Domain.Entities;

public sealed class RecoveryToken
{
    private RecoveryToken()
    {
    }

    private RecoveryToken(Guid id, Guid accountId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = id;
        AccountId = accountId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public string TokenHash { get; private set; } = "";

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsUsable(DateTimeOffset now) => UsedAt is null && ExpiresAt > now;

    public void MarkUsed(DateTimeOffset now) => UsedAt = now;

    public static RecoveryToken Issue(Guid id, Guid accountId, string tokenHash, DateTimeOffset expiresAt) =>
        new(id, accountId, tokenHash, expiresAt);
}
