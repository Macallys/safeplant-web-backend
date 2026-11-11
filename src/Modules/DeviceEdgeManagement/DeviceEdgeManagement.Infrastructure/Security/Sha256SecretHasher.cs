using System.Security.Cryptography;
using System.Text;
using DeviceEdgeManagement.Domain.Interfaces;

namespace DeviceEdgeManagement.Infrastructure.Security;

public sealed class Sha256SecretHasher : ISecretHasher
{
    public string Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
