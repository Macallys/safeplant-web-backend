using IAM.Domain.Entities;
using IAM.Domain.Services;

namespace IAM.Interface.Assemblers;

public static class IamAssembler
{
    public static SignInResponse ToSignInResponse(SignInResult result) =>
        new(result.AccessToken, result.Role, result.ExpiresAt);

    public static UserAccountResponse ToUserAccount(UserAccount account) =>
        new(account.Id.ToString(), account.Email, account.Role.ToString(), account.Enabled);

    public static UserListResponse ToUserList(IReadOnlyList<UserAccount> accounts) =>
        new(accounts.Select(ToUserAccount).ToArray());
}
