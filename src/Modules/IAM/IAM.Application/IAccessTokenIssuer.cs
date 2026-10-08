using IAM.Domain;

namespace IAM.Application;

public interface IAccessTokenIssuer
{
    string Issue(UserAccount account, Session session);
}
