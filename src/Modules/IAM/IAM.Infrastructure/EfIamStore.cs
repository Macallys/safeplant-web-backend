using IAM.Application;
using IAM.Domain;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure;

public sealed class EfIamStore : IIamStore
{
    private readonly IamDbContext _db;

    public EfIamStore(IamDbContext db)
    {
        _db = db;
    }

    public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Accounts.SingleOrDefaultAsync(account => account.Email == email, cancellationToken);

    public Task<UserAccount?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Accounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Accounts.OrderBy(account => account.Email).ToListAsync(cancellationToken);

    public async Task AddAccountAsync(UserAccount account, CancellationToken cancellationToken) =>
        await _db.Accounts.AddAsync(account, cancellationToken);

    public async Task AddSessionAsync(Session session, CancellationToken cancellationToken) =>
        await _db.Sessions.AddAsync(session, cancellationToken);

    public Task<Session?> FindSessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        _db.Sessions.SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public async Task InvalidateUnusedRecoveryTokensAsync(
        Guid accountId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var pending = await _db.RecoveryTokens
            .Where(token => token.AccountId == accountId && token.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in pending)
        {
            token.MarkUsed(now);
        }
    }

    public async Task AddRecoveryTokenAsync(RecoveryToken token, CancellationToken cancellationToken) =>
        await _db.RecoveryTokens.AddAsync(token, cancellationToken);

    public Task<RecoveryToken?> FindRecoveryTokenByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _db.RecoveryTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
