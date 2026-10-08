namespace IAM.Domain;

public sealed class Session
{
    private Session()
    {
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Channel Channel { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public static Session Open(Guid id, Guid accountId, Channel channel, DateTimeOffset expiresAt) =>
        new()
        {
            Id = id,
            AccountId = accountId,
            Channel = channel,
            ExpiresAt = expiresAt
        };
}
