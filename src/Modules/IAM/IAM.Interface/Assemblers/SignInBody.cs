namespace IAM.Interface.Assemblers;

public sealed record SignInBody(string? Email, string? Password, string? Channel);
