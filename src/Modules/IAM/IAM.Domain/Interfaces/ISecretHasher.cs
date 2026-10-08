namespace IAM.Domain.Interfaces;

public interface ISecretHasher
{
    string Hash(string value);
}
