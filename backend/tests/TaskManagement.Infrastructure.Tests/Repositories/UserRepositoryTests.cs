using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using Xunit;

namespace TaskManagement.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _sut = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_AddsUserToDatabase()
    {
        var user = User.Create("test@example.com", "hash");
        var result = await _sut.CreateAsync(user);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(1, await _context.Users.CountAsync());
    }

    [Fact]
    public async Task GetByEmailAsync_FindsUser()
    {
        await _sut.CreateAsync(User.Create("test@example.com", "hash"));
        var result = await _sut.GetByEmailAsync("test@example.com");
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive()
    {
        await _sut.CreateAsync(User.Create("test@example.com", "hash"));
        var result = await _sut.GetByEmailAsync("TEST@EXAMPLE.COM");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetByEmailAsync("missing@example.com");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_FindsUser()
    {
        var user = await _sut.CreateAsync(User.Create("test@example.com", "hash"));
        var result = await _sut.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
