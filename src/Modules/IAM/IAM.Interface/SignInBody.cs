namespace IAM.Interface;

public sealed record SignInBody(string? Email, string? Password, string? Channel);
