using System.Security.Cryptography;
using System.Text;

namespace DeviceEdgeManagement.Domain.Services;

public static class PlantKeys
{
    public static bool Matches(string expected, string? presented)
    {
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var presentedBytes = Encoding.UTF8.GetBytes(presented);
        if (expectedBytes.Length != presentedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, presentedBytes);
    }
}
