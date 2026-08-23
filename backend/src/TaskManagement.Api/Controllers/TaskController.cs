using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _taskService.ListTasksAsync(UserId);
        return Ok(tasks.Select(MapToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var errors = ValidateTaskRequest(request);
        if (errors.Count > 0) return BadRequest(new { title = "Validation failed", status = 400, errors });

        var status = ParseStatus(request.Status);
        var task = await _taskService.CreateTaskAsync(request.Title, request.Description, request.DueDate, UserId);
        if (request.Status != null)
        {
            task.Update(task.Title, task.Description, status, task.DueDate);
            task = await _taskService.UpdateTaskAsync(task.Id, task.Title, task.Description, status, task.DueDate, UserId);
        }
        return StatusCode(201, MapToDto(task));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _taskService.GetTaskAsync(id, UserId);
        if (task == null) return NotFound(new { title = "Not found", status = 404, detail = "Task not found." });
        return Ok(MapToDto(task));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var status = ParseStatus(request.Status);
        try
        {
            var task = await _taskService.UpdateTaskAsync(id, request.Title, request.Description, status, request.DueDate, UserId);
            return Ok(MapToDto(task));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { title = "Not found", status = 404, detail = "Task not found." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _taskService.DeleteTaskAsync(id, UserId);
        if (!deleted) return NotFound(new { title = "Not found", status = 404, detail = "Task not found." });
        return NoContent();
    }

    private static TaskStatus ParseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "pending" => TaskStatus.Pending,
        "inprogress" => TaskStatus.InProgress,
        "completed" => TaskStatus.Completed,
        _ => throw new ArgumentException($"Invalid status: {status}")
    };

    private static Dictionary<string, List<string>> ValidateTaskRequest(CreateTaskRequest request)
    {
        var errors = new Dictionary<string, List<string>>();
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 120)
            errors["title"] = new List<string> { "Title is required and must be at most 120 characters." };
        if (request.Description != null && request.Description.Trim().Length > 2000)
            errors["description"] = new List<string> { "Description must be at most 2000 characters." };
        return errors;
    }

    private static TaskDto MapToDto(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt);
}
