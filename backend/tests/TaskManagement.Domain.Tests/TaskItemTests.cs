using TaskManagement.Domain.Entities;
using Xunit;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void Create_WithValidData_SetsFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var task = TaskItem.Create("Test Title", "Test Description", null, userId);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Test Title", task.Title);
        Assert.Equal("Test Description", task.Description);
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.Null(task.DueDate);
        Assert.Equal(userId, task.UserId);
    }

    [Fact]
    public void Create_TrimsTitle()
    {
        var task = TaskItem.Create("  Trimmed Title  ", null, null, Guid.NewGuid());
        Assert.Equal("Trimmed Title", task.Title);
    }

    [Fact]
    public void Create_NullDescription_BecomesNull()
    {
        var task = TaskItem.Create("Title", null, null, Guid.NewGuid());
        Assert.Null(task.Description);
    }

    [Fact]
    public void Create_BlankDescription_BecomesNull()
    {
        var task = TaskItem.Create("Title", "   ", null, Guid.NewGuid());
        Assert.Null(task.Description);
    }

    [Fact]
    public void Create_TrimmedDescription_PreservesInternalWhitespace()
    {
        var task = TaskItem.Create("Title", "  Hello  World  ", null, Guid.NewGuid());
        Assert.Equal("Hello  World", task.Description);
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var task = TaskItem.Create("Original", null, null, Guid.NewGuid());
        task.Update("Updated Title", "Updated Desc", TaskStatus.InProgress, DateTime.UtcNow);

        Assert.Equal("Updated Title", task.Title);
        Assert.Equal("Updated Desc", task.Description);
        Assert.Equal(TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void Update_NullDescription_ClearsDescription()
    {
        var task = TaskItem.Create("Title", "Has desc", null, Guid.NewGuid());
        task.Update("Title", null, TaskStatus.Pending, null);

        Assert.Null(task.Description);
    }
}
