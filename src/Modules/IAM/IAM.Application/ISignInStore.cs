using IAM.Domain;

namespace IAM.Application;

public interface ISignInStore
{
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task AddSessionAsync(Session session, CancellationToken cancellationToken);
}
