using Microsoft.Extensions.Configuration;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Infrastructure.Tests.Services;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _sut;

    public JwtTokenGeneratorTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();
        _sut = new JwtTokenGenerator(config);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "test@example.com");
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void GenerateToken_ContainsValidClaims()
    {
        var userId = Guid.NewGuid();
        var token = _sut.GenerateToken(userId, "test@example.com");
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("test@example.com", jwt.Claims.First(c => c.Type == "email").Value);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "test@example.com");
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains("TestAudience", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresIn60Minutes()
    {
        var token = _sut.GenerateToken(Guid.NewGuid(), "test@example.com");
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
        Assert.True(Math.Abs((jwt.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }
}
