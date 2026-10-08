namespace IAM.Interface.Assemblers;

public sealed record SignInResponse(string AccessToken, string Role, DateTimeOffset ExpiresAt);
