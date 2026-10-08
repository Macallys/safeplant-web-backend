using System.Security.Cryptography;
using System.Text;
using IAM.Domain.Interfaces;

namespace IAM.Infrastructure.Security;

public sealed class Sha256SecretHasher : ISecretHasher
{
    public string Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
