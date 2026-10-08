namespace IAM.Domain;

public sealed class Session
{
    private Session()
    {
    }

    private Session(Guid id, Guid accountId, Channel channel, DateTimeOffset expiresAt)
    {
        Id = id;
        AccountId = accountId;
        Channel = channel;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Channel Channel { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public bool IsOpen(DateTimeOffset now) => ClosedAt is null && ExpiresAt > now;

    public void Close(DateTimeOffset now) => ClosedAt = now;

    public static Session Open(Guid id, Guid accountId, Channel channel, DateTimeOffset expiresAt) =>
        new(id, accountId, channel, expiresAt);
}
