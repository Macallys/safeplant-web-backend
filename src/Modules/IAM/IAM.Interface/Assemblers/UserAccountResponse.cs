namespace IAM.Interface.Assemblers;

public sealed record UserAccountResponse(string Id, string Email, string Role, bool Enabled);
