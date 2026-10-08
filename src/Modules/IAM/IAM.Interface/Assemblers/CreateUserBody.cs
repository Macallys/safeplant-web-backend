namespace IAM.Interface.Assemblers;

public sealed record CreateUserBody(string? Email, string? Password, string? Role);
