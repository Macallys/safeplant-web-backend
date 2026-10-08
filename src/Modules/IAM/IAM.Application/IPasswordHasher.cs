namespace IAM.Application;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Matches(string password, string passwordHash);
}
