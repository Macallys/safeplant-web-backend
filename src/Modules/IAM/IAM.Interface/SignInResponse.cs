namespace IAM.Interface;

public sealed record SignInResponse(string AccessToken, string Role, DateTimeOffset ExpiresAt);
