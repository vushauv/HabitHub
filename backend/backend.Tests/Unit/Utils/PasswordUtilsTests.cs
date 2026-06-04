using backend.Utils;
using Xunit;

namespace backend.Tests.Unit.Utils;

public class PasswordUtilsTests
{
    [Fact]
    public void HashPassword_ReturnsStringWithSaltPrefixed()
    {
        var result = PasswordUtils.HashPassword("password", "somepepper");
        Assert.True(result.Length > 32); // 32 hex salt + base64 hash
        Assert.True(result[..32].All(c => "0123456789ABCDEF".Contains(c)));
    }

    [Fact]
    public void HashPassword_ProducesDifferentHashEachCall()
    {
        var hash1 = PasswordUtils.HashPassword("password", "pepper");
        var hash2 = PasswordUtils.HashPassword("password", "pepper");
        Assert.NotEqual(hash1, hash2); // different random salts
    }

    [Fact]
    public void VerifyPassword_CorrectPasswordReturnsTrue()
    {
        var stored = PasswordUtils.HashPassword("mypassword", "testpepper");
        Assert.True(PasswordUtils.VerifyPassword("mypassword", stored, "testpepper"));
    }

    [Fact]
    public void VerifyPassword_WrongPasswordReturnsFalse()
    {
        var stored = PasswordUtils.HashPassword("mypassword", "testpepper");
        Assert.False(PasswordUtils.VerifyPassword("wrongpassword", stored, "testpepper"));
    }

    [Fact]
    public void VerifyPassword_WrongPepperReturnsFalse()
    {
        var stored = PasswordUtils.HashPassword("mypassword", "correctpepper");
        Assert.False(PasswordUtils.VerifyPassword("mypassword", stored, "wrongpepper!"));
    }
}
