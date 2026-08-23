using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_SetsFieldsCorrectly()
    {
        var user = User.Create("Test@Example.com", "hash123");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
    }

    [Fact]
    public void Create_NormalizesEmailToLowercase()
    {
        var user = User.Create("USER@DOMAIN.COM", "hash");
        Assert.Equal("user@domain.com", user.Email);
    }

    [Fact]
    public void Create_TrimmedEmail()
    {
        var user = User.Create("  user@example.com  ", "hash");
        Assert.Equal("user@example.com", user.Email);
    }
}
