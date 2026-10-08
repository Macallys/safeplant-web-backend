using System.Security.Cryptography;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Services;
using Microsoft.Extensions.Logging;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace IAM.Application.Services;

public sealed class RecoveryService : IRecoveryService
{
    private readonly IIamStore _store;
    private readonly IPasswordHasher _passwords;
    private readonly ISecretHasher _secrets;
    private readonly IClock _clock;
    private readonly TimeSpan _lifetime;
    private readonly ILogger<RecoveryService> _logger;

    public RecoveryService(
        IIamStore store,
        IPasswordHasher passwords,
        ISecretHasher secrets,
        IClock clock,
        TimeSpan lifetime,
        ILogger<RecoveryService> logger)
    {
        _store = store;
        _passwords = passwords;
        _secrets = secrets;
        _clock = clock;
        _lifetime = lifetime;
        _logger = logger;
    }

    public async Task RequestAsync(string? email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? "";
        var account = normalizedEmail.Length == 0
            ? null
            : await _store.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (account is null || !account.Enabled)
        {
            return;
        }

        var now = _clock.UtcNow;
        await _store.InvalidateUnusedRecoveryTokensAsync(account.Id, now, cancellationToken);

        var clear = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        _logger.LogInformation("Recovery token for {Email}: {Token}", account.Email, clear);

        var token = RecoveryToken.Issue(
            Guid.NewGuid(),
            account.Id,
            _secrets.Hash(clear),
            now.Add(_lifetime));
        await _store.AddRecoveryTokenAsync(token, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(string? token, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrEmpty(password))
        {
            throw new AppException("recovery_expired", 422);
        }

        var stored = await _store.FindRecoveryTokenByHashAsync(_secrets.Hash(token), cancellationToken);
        var now = _clock.UtcNow;
        if (stored is null || !stored.IsUsable(now))
        {
            throw new AppException("recovery_expired", 422);
        }

        var account = await _store.FindByIdAsync(stored.AccountId, cancellationToken);
        if (account is null || !account.Enabled)
        {
            throw new AppException("recovery_expired", 422);
        }

        stored.MarkUsed(now);
        account.ChangePassword(_passwords.Hash(password));
        await _store.SaveChangesAsync(cancellationToken);
    }
}
