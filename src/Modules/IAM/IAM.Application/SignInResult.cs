namespace IAM.Application;

public sealed record SignInResult(string AccessToken, string Role, DateTimeOffset ExpiresAt);
