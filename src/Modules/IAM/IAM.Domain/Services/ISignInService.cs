namespace IAM.Domain.Services;

public interface ISignInService
{
    Task<SignInResult> SignIn(
        string? email,
        string? password,
        string? channel,
        CancellationToken cancellationToken);
}
