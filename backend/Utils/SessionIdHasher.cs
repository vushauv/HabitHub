using System.Security.Cryptography;
using System.Text;

namespace backend.Utils;

public static class SessionIdHasher
{
    public static string Hash(string rawSessionId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawSessionId)));
}
