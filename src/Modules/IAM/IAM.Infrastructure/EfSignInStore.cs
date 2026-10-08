using IAM.Application;
using IAM.Domain;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure;

public sealed class EfSignInStore : ISignInStore
{
    private readonly IamDbContext _db;

    public EfSignInStore(IamDbContext db)
    {
        _db = db;
    }

    public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Accounts.SingleOrDefaultAsync(account => account.Email == email, cancellationToken);

    public async Task AddSessionAsync(Session session, CancellationToken cancellationToken)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
