namespace IAM.Interface;

public sealed record UserAccountResponse(string Id, string Email, string Role, bool Enabled);
