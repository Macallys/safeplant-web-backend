using IAM.Domain.Entities;

namespace IAM.Domain.Interfaces;

public interface IAccessTokenIssuer
{
    string Issue(UserAccount account, Session session);
}
