using System.Security.Cryptography;
using System.Text;

namespace backend.Utils;

public static class PasswordUtils
{
    private const int Iterations = 10_000;
    private const int KeySize = 32;
    private const int SaltByteLength = 16;
    private const int SaltHexLength = SaltByteLength * 2;

    public static string HashPassword(string password, string pepper)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltByteLength);
        var salt = Convert.ToHexString(saltBytes);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: Encoding.UTF8.GetBytes(password + pepper),
            salt: saltBytes,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA512);
        return salt + Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
    }

    public static bool VerifyPassword(string inputPassword, string storedValue, string pepper)
    {
        if (storedValue.Length <= SaltHexLength) return false;
        var salt = storedValue[..SaltHexLength];
        var storedHash = storedValue[SaltHexLength..];

        var saltBytes = Convert.FromHexString(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: Encoding.UTF8.GetBytes(inputPassword + pepper),
            salt: saltBytes,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA512);
        var computed = pbkdf2.GetBytes(KeySize);

        return CryptographicOperations.FixedTimeEquals(computed, Convert.FromBase64String(storedHash));
    }
}
