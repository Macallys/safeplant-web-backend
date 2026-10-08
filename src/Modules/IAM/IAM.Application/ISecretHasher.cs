namespace IAM.Application;

public interface ISecretHasher
{
    string Hash(string value);
}
