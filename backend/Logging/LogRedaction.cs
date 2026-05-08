using System.Security.Cryptography;
using System.Text;

namespace backend.Logging;

public static class LogRedaction
{
    public static string Fingerprint(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)))[..12];
}
