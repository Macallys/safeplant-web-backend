using IAM.Domain.Entities;

namespace IAM.Domain.Services;

public interface IDirectoryService
{
    Task<UserAccount> CreateAsync(
        AccessSession actor,
        string? email,
        string? password,
        string? role,
        CancellationToken cancellationToken);

    Task<UserAccount> AssignRoleAsync(
        AccessSession actor,
        string? accountId,
        string? role,
        CancellationToken cancellationToken);

    Task<UserAccount> SetEnabledAsync(
        AccessSession actor,
        string? accountId,
        bool enabled,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserAccount>> ListAsync(AccessSession actor, CancellationToken cancellationToken);
}
