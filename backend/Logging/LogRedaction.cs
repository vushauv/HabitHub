using System.Security.Cryptography;
using System.Text;

namespace backend.Logging;

public static class LogRedaction
{
    private static string _pepper = string.Empty;

    public static void Configure(string pepper) => _pepper = pepper;

    public static string Fingerprint(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token + _pepper)))[..12];
}
