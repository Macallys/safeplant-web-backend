namespace IAM.Domain.Services;

public interface ISessionAccess
{
    Task<AccessSession> RequireAsync(string? bearer, CancellationToken cancellationToken);

    Task LogoutAsync(AccessSession access, CancellationToken cancellationToken);
}
