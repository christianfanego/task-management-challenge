using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Infrastructure.Tests.Services;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        var hash = _sut.HashPassword("Password1!");
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual("Password1!", hash);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("Password1!");
        Assert.True(_sut.VerifyPassword("Password1!", hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Password1!");
        Assert.False(_sut.VerifyPassword("WrongPassword!", hash));
    }

    [Fact]
    public void HashPassword_DifferentHashesForSamePassword()
    {
        var hash1 = _sut.HashPassword("Password1!");
        var hash2 = _sut.HashPassword("Password1!");
        Assert.NotEqual(hash1, hash2);
    }
}
