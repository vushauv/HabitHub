using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace backend.Utils
{
    public static class InviteCodeGenerator
    {
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmlnopqrstuvwxyz0123456789";
        private const int CodeLength = 8;
        public static string GenerateInviteCodeValue()
        {
            char[] buffer = new char[CodeLength];

            for(int i = 0; i< CodeLength; i++)
            {
                int index = RandomNumberGenerator.GetInt32(AllowedChars.Length);
                buffer[i] = AllowedChars[index];
            }

            return new string(buffer);
        }
    }
}
