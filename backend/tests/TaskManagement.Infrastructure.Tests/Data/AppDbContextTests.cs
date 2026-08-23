using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Xunit;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Data;

public class AppDbContextTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CanCreateAndQueryUser()
    {
        using var context = CreateInMemoryContext();
        var user = User.Create("test@example.com", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await context.Users.FirstAsync();
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task CanCreateAndQueryTask()
    {
        using var context = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        context.Users.Add(new User { Id = userId, Email = "u@e.com", PasswordHash = "h" });
        var task = TaskItem.Create("Title", "Desc", null, userId);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var result = await context.Tasks.FirstAsync();
        Assert.Equal("Title", result.Title);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task Status_StoredAsString()
    {
        using var context = CreateInMemoryContext();
        var task = TaskItem.Create("Title", null, null, Guid.NewGuid());
        task.Status = TaskStatus.InProgress;
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var result = await context.Tasks.FirstAsync();
        Assert.Equal(TaskStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task User_EmailIndex_IsUnique()
    {
        using var context = CreateInMemoryContext();
        context.Users.Add(new User { Id = Guid.NewGuid(), Email = "dup@example.com", PasswordHash = "h1" });
        context.Users.Add(new User { Id = Guid.NewGuid(), Email = "dup@example.com", PasswordHash = "h2" });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Task_DueDate_IsNullable()
    {
        using var context = CreateInMemoryContext();
        var task = TaskItem.Create("Title", null, null, Guid.NewGuid());
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        var result = await context.Tasks.FirstAsync();
        Assert.Null(result.DueDate);
    }

    [Fact]
    public async Task Task_Description_Null_StoredCorrectly()
    {
        using var context = CreateInMemoryContext();
        var task = TaskItem.Create("Title", null, null, Guid.NewGuid());
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        var result = await context.Tasks.FirstAsync();
        Assert.Null(result.Description);
    }
}
