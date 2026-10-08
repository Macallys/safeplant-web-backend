namespace IAM.Domain.Services;

public sealed record SignInResult(string AccessToken, string Role, DateTimeOffset ExpiresAt);
