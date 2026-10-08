using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.Services;
using IAM.Domain.ValueObjects;
using SharedKernel.Application;

namespace IAM.Application.Services;

public sealed class DirectoryService : IDirectoryService
{
    private readonly IIamStore _store;
    private readonly IPasswordHasher _passwords;

    public DirectoryService(IIamStore store, IPasswordHasher passwords)
    {
        _store = store;
        _passwords = passwords;
    }

    public async Task<UserAccount> CreateAsync(
        AccessSession actor,
        string? email,
        string? password,
        string? role,
        CancellationToken cancellationToken)
    {
        RequireManager(actor);

        if (!TryParseRole(role, out var parsedRole))
        {
            throw new AppException("role_not_allowed", 403);
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            throw new AppException("email_already_exists", 409);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await _store.FindByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new AppException("email_already_exists", 409);
        }

        var account = new UserAccount(
            Guid.NewGuid(),
            normalizedEmail,
            _passwords.Hash(password),
            parsedRole,
            enabled: true);
        await _store.AddAccountAsync(account, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<UserAccount> AssignRoleAsync(
        AccessSession actor,
        string? accountId,
        string? role,
        CancellationToken cancellationToken)
    {
        RequireManager(actor);
        var account = await FindAccountAsync(accountId, cancellationToken);
        if (!TryParseRole(role, out var parsedRole))
        {
            throw new AppException("role_not_allowed", 403);
        }

        account.ChangeRole(parsedRole);
        await _store.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<UserAccount> SetEnabledAsync(
        AccessSession actor,
        string? accountId,
        bool enabled,
        CancellationToken cancellationToken)
    {
        RequireManager(actor);
        var account = await FindAccountAsync(accountId, cancellationToken);
        account.SetEnabled(enabled);
        await _store.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IReadOnlyList<UserAccount>> ListAsync(
        AccessSession actor,
        CancellationToken cancellationToken)
    {
        RequireManager(actor);
        return await _store.ListAsync(cancellationToken);
    }

    private async Task<UserAccount> FindAccountAsync(string? accountId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(accountId, out var id))
        {
            throw new AppException("account_not_found", 404);
        }

        var account = await _store.FindByIdAsync(id, cancellationToken);
        if (account is null)
        {
            throw new AppException("account_not_found", 404);
        }

        return account;
    }

    private static void RequireManager(AccessSession actor)
    {
        if (actor.Role != Role.PlantManager || actor.Channel != Channel.Web)
        {
            throw new AppException("role_not_allowed", 403);
        }
    }

    private static bool TryParseRole(string? value, out Role role)
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
}
