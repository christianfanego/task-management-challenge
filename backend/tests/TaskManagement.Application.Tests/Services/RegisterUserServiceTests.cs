using Moq;
using TaskManagement.Application.Ports;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Application.Tests.Services;

public class RegisterUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterUserService _sut;

    public RegisterUserServiceTests()
    {
        _passwordHasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed-password");
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _sut = new RegisterUserService(_userRepository.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUser()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var user = await _sut.RegisterAsync("test@example.com", "Password1!");

        Assert.Equal("test@example.com", user.Email);
        _userRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperation()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "existing@example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RegisterAsync("existing@example.com", "Password1!"));
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _sut.RegisterAsync("test@example.com", "Password1!");

        _passwordHasher.Verify(h => h.HashPassword("Password1!"), Times.Once);
    }
}
