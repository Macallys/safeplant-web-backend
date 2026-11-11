namespace DeviceEdgeManagement.Domain.Interfaces;

public interface ISecretHasher
{
    string Hash(string value);
}
