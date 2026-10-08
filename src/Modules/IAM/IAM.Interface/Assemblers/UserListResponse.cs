namespace IAM.Interface.Assemblers;

public sealed record UserListResponse(IReadOnlyList<UserAccountResponse> Items);
