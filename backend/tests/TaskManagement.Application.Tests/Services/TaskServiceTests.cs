using Moq;
using TaskManagement.Application.Ports;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using Xunit;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);
        _taskRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);
        _sut = new TaskService(_taskRepository.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidData_CreatesTask()
    {
        var userId = Guid.NewGuid();
        var task = await _sut.CreateTaskAsync("Title", "Desc", null, userId);

        Assert.Equal("Title", task.Title);
        Assert.Equal(userId, task.UserId);
        _taskRepository.Verify(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskAsync_ExistingTask_ReturnsTask()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expected = new TaskItem { Id = taskId, UserId = userId };
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetTaskAsync(taskId, userId);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetTaskAsync_NonexistentTask_ReturnsNull()
    {
        _taskRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetTaskAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_ExistingTask_UpdatesAndReturns()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = TaskItem.Create("Old", null, null, userId);
        _taskRepository.Setup(r => r.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateTaskAsync(taskId, "New", "Desc", TaskStatus.InProgress, null, userId);

        Assert.Equal("New", result!.Title);
        Assert.Equal(TaskStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateTaskAsync_NonexistentTask_ThrowsKeyNotFound()
    {
        _taskRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateTaskAsync(Guid.NewGuid(), "Title", null, TaskStatus.Pending, null, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteTaskAsync_DelegatesToRepository()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _taskRepository.Setup(r => r.DeleteAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.DeleteTaskAsync(taskId, userId);

        Assert.True(result);
    }
}
