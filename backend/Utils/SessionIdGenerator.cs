using System.Security.Cryptography;
namespace backend.Utils
{
    public static class SessionIdGenerator
    {
        public static string GenerateSessionId()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes);
        }
    }
}
