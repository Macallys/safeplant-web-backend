using IAM.Domain;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace IAM.Application;

public sealed class SessionAccess
{
    private readonly IIamStore _store;
    private readonly IAccessTokenReader _tokens;
    private readonly IClock _clock;

    public SessionAccess(IIamStore store, IAccessTokenReader tokens, IClock clock)
    {
        _store = store;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<AccessSession> RequireAsync(string? bearer, CancellationToken cancellationToken)
    {
        var claims = string.IsNullOrWhiteSpace(bearer) ? null : _tokens.Read(bearer);
        if (claims is null
            || !TryParseRole(claims.Role, out var role)
            || !TryParseChannel(claims.Channel, out var channel))
        {
            throw new AppException("unauthorized", 401);
        }

        var session = await _store.FindSessionAsync(claims.SessionId, cancellationToken);
        if (session is null || session.AccountId != claims.AccountId || !session.IsOpen(_clock.UtcNow))
        {
            throw new AppException("unauthorized", 401);
        }

        var account = await _store.FindByIdAsync(claims.AccountId, cancellationToken);
        if (account is null || !account.Enabled)
        {
            throw new AppException("unauthorized", 401);
        }

        return new AccessSession(account.Id, session.Id, role, channel);
    }

    public async Task LogoutAsync(AccessSession access, CancellationToken cancellationToken)
    {
        var session = await _store.FindSessionAsync(access.SessionId, cancellationToken);
        if (session is null || !session.IsOpen(_clock.UtcNow))
        {
            throw new AppException("unauthorized", 401);
        }

        session.Close(_clock.UtcNow);
        await _store.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseRole(string value, out Role role)
    {
        if (value == nameof(Role.PlantManager))
        {
            role = Role.PlantManager;
            return true;
        }

        if (value == nameof(Role.Supervisor))
        {
            role = Role.Supervisor;
            return true;
        }

        role = default;
        return false;
    }

    private static bool TryParseChannel(string value, out Channel channel)
    {
        if (value == nameof(Channel.Web))
        {
            channel = Channel.Web;
            return true;
        }

        if (value == nameof(Channel.Mobile))
        {
            channel = Channel.Mobile;
            return true;
        }

        channel = default;
        return false;
    }
}
