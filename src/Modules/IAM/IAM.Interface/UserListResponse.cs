namespace IAM.Interface;

public sealed record UserListResponse(IReadOnlyList<UserAccountResponse> Items);
