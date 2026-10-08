namespace IAM.Domain.Interfaces;

public interface IAccessTokenReader
{
    AccessTokenClaims? Read(string token);
}

public sealed record AccessTokenClaims(Guid AccountId, Guid SessionId, string Role, string Channel);
