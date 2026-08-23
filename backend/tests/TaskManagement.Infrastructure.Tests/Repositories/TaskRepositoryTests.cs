using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using Xunit;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _context.Users.Add(new User { Id = _userId, Email = "u@e.com", PasswordHash = "h" });
        _context.SaveChanges();
        _sut = new TaskRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_AddsTaskToDatabase()
    {
        var task = TaskItem.Create("Title", null, null, _userId);
        var result = await _sut.CreateAsync(task);
        Assert.Equal("Title", result.Title);
        Assert.Equal(1, await _context.Tasks.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOwnedTask()
    {
        var task = TaskItem.Create("Title", null, null, _userId);
        await _sut.CreateAsync(task);
        var result = await _sut.GetByIdAsync(task.Id, _userId);
        Assert.NotNull(result);
        Assert.Equal("Title", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForForeignTask()
    {
        var task = TaskItem.Create("Title", null, null, _userId);
        await _sut.CreateAsync(task);
        var result = await _sut.GetByIdAsync(task.Id, Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsOnlyUserTasks()
    {
        await _sut.CreateAsync(TaskItem.Create("My Task", null, null, _userId));
        await _sut.CreateAsync(TaskItem.Create("Other Task", null, null, Guid.NewGuid()));
        var result = await _sut.GetAllByUserIdAsync(_userId);
        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = TaskItem.Create("Old", null, null, _userId);
        await _sut.CreateAsync(task);
        task.Update("New", "Desc", TaskStatus.Completed, null);
        await _sut.UpdateAsync(task);
        var result = await _sut.GetByIdAsync(task.Id, _userId);
        Assert.Equal("New", result!.Title);
        Assert.Equal(TaskStatus.Completed, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedTask()
    {
        var task = TaskItem.Create("Title", null, null, _userId);
        await _sut.CreateAsync(task);
        var result = await _sut.DeleteAsync(task.Id, _userId);
        Assert.True(result);
        Assert.Null(await _sut.GetByIdAsync(task.Id, _userId));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_ForForeignTask()
    {
        var task = TaskItem.Create("Title", null, null, _userId);
        await _sut.CreateAsync(task);
        var result = await _sut.DeleteAsync(task.Id, Guid.NewGuid());
        Assert.False(result);
        Assert.NotNull(await _sut.GetByIdAsync(task.Id, _userId));
    }
}
