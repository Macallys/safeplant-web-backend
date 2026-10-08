namespace IAM.Application;

public interface IAccessTokenReader
{
    AccessTokenClaims? Read(string token);
}

public sealed record AccessTokenClaims(Guid AccountId, Guid SessionId, string Role, string Channel);
