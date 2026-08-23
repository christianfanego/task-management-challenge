using TaskManagement.Application.Ports;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskItem> CreateTaskAsync(string title, string? description, DateTime? dueDate, Guid userId, CancellationToken ct = default)
    {
        var task = TaskItem.Create(title, description, dueDate, userId);
        return await _taskRepository.CreateAsync(task, ct);
    }

    public async Task<TaskItem?> GetTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        return await _taskRepository.GetByIdAsync(taskId, userId, ct);
    }

    public async Task<IReadOnlyList<TaskItem>> ListTasksAsync(Guid userId, CancellationToken ct = default)
    {
        return await _taskRepository.GetAllByUserIdAsync(userId, ct);
    }

    public async Task<TaskItem> UpdateTaskAsync(Guid taskId, string title, string? description, TaskStatus status, DateTime? dueDate, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, userId, ct);
        if (task == null)
            throw new KeyNotFoundException("Task not found.");

        task.Update(title, description, status, dueDate);
        return await _taskRepository.UpdateAsync(task, ct);
    }

    public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        return await _taskRepository.DeleteAsync(taskId, userId, ct);
    }
}
