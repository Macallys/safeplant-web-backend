using IAM.Domain;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace IAM.Application;

public sealed class SignInService
{
    private readonly ISignInStore _store;
    private readonly IPasswordHasher _passwords;
    private readonly IAccessTokenIssuer _tokens;
    private readonly IClock _clock;
    private readonly TimeSpan _sessionLifetime;

    public SignInService(
        ISignInStore store,
        IPasswordHasher passwords,
        IAccessTokenIssuer tokens,
        IClock clock,
        TimeSpan sessionLifetime)
    {
        _store = store;
        _passwords = passwords;
        _tokens = tokens;
        _clock = clock;
        _sessionLifetime = sessionLifetime;
    }

    public async Task<SignInResult> SignIn(
        string? email,
        string? password,
        string? channel,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? "";
        var account = normalizedEmail.Length == 0
            ? null
            : await _store.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (account is null || password is null || !_passwords.Matches(password, account.PasswordHash))
        {
            throw new AppException("invalid_credentials", 401);
        }

        if (!account.Enabled)
        {
            throw new AppException("account_disabled", 403);
        }

        if (!TryParseChannel(channel, out var parsedChannel) || !account.Accepts(parsedChannel))
        {
            throw new AppException("role_not_allowed", 403);
        }

        var expiresAt = _clock.UtcNow.Add(_sessionLifetime);
        var session = Session.Open(Guid.NewGuid(), account.Id, parsedChannel, expiresAt);
        await _store.AddSessionAsync(session, cancellationToken);

        return new SignInResult(_tokens.Issue(account, session), account.Role.ToString(), expiresAt);
    }

    private static bool TryParseChannel(string? value, out Channel channel)
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
