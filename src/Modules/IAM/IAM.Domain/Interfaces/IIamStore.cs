using IAM.Domain.Entities;

namespace IAM.Domain.Interfaces;

public interface IIamStore
{
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken);

    Task AddAccountAsync(UserAccount account, CancellationToken cancellationToken);

    Task AddSessionAsync(Session session, CancellationToken cancellationToken);

    Task<Session?> FindSessionAsync(Guid sessionId, CancellationToken cancellationToken);

    Task InvalidateUnusedRecoveryTokensAsync(Guid accountId, DateTimeOffset now, CancellationToken cancellationToken);

    Task AddRecoveryTokenAsync(RecoveryToken token, CancellationToken cancellationToken);

    Task<RecoveryToken?> FindRecoveryTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
