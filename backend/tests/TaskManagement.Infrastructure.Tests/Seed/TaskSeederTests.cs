using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Seed;
using Xunit;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Seed;

public class TaskSeederTests : IDisposable
{
    private readonly AppDbContext _context;

    public TaskSeederTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void Seed_CreatesOneUserAndThreeTasks()
    {
        TaskSeeder.Seed(_context);
        Assert.Equal(1, _context.Users.Count());
        Assert.Equal(3, _context.Tasks.Count());
    }

    [Fact]
    public void Seed_IsIdempotent()
    {
        TaskSeeder.Seed(_context);
        TaskSeeder.Seed(_context);
        Assert.Equal(1, _context.Users.Count());
        Assert.Equal(3, _context.Tasks.Count());
    }

    [Fact]
    public void Seed_CreatesDemoUserWithFixedId()
    {
        TaskSeeder.Seed(_context);
        var user = _context.Users.First();
        Assert.Equal(TaskSeeder.DemoUserId, user.Id);
        Assert.Equal("demo@example.com", user.Email);
    }

    [Fact]
    public void Seed_CreatesTasksWithFixedIds()
    {
        TaskSeeder.Seed(_context);
        Assert.True(_context.Tasks.Any(t => t.Id == TaskSeeder.Task1Id));
        Assert.True(_context.Tasks.Any(t => t.Id == TaskSeeder.Task2Id));
        Assert.True(_context.Tasks.Any(t => t.Id == TaskSeeder.Task3Id));
    }

    [Fact]
    public void Seed_CreatesTasksWithCorrectStatuses()
    {
        TaskSeeder.Seed(_context);
        var task1 = _context.Tasks.First(t => t.Id == TaskSeeder.Task1Id);
        var task2 = _context.Tasks.First(t => t.Id == TaskSeeder.Task2Id);
        var task3 = _context.Tasks.First(t => t.Id == TaskSeeder.Task3Id);
        Assert.Equal(TaskStatus.Pending, task1.Status);
        Assert.Equal(TaskStatus.InProgress, task2.Status);
        Assert.Equal(TaskStatus.Completed, task3.Status);
    }

    [Fact]
    public void Seed_AllTasksBelongToDemoUser()
    {
        TaskSeeder.Seed(_context);
        Assert.All(_context.Tasks, t => Assert.Equal(TaskSeeder.DemoUserId, t.UserId));
    }

    [Fact]
    public void Seed_DoesNotAlterExistingData()
    {
        TaskSeeder.Seed(_context);
        var user = _context.Users.First();
        user.Email = "changed@example.com";
        _context.SaveChanges();

        TaskSeeder.Seed(_context);
        Assert.Equal("changed@example.com", _context.Users.First().Email);
    }
}
