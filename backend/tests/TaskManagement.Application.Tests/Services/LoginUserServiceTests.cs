using Moq;
using TaskManagement.Application.Ports;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Application.Tests.Services;

public class LoginUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly LoginUserService _sut;

    public LoginUserServiceTests()
    {
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>())).Returns("jwt-token");
        _sut = new LoginUserService(_userRepository.Object, _passwordHasher.Object, _jwtTokenGenerator.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };
        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("Password1!", "hash")).Returns(true);

        var token = await _sut.LoginAsync("test@example.com", "Password1!");

        Assert.Equal("jwt-token", token);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };
        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("wrong", "hash")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync("test@example.com", "wrong"));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorized()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync("unknown@example.com", "Password1!"));
    }
}
