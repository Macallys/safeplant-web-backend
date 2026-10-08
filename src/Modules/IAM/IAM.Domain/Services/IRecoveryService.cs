namespace IAM.Domain.Services;

public interface IRecoveryService
{
    Task RequestAsync(string? email, CancellationToken cancellationToken);

    Task ResetAsync(string? token, string? password, CancellationToken cancellationToken);
}
