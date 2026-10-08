namespace IAM.Interface;

public sealed record CreateUserBody(string? Email, string? Password, string? Role);
