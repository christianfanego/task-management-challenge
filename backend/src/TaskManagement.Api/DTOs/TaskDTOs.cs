namespace TaskManagement.Api.DTOs;

public record CreateTaskRequest(string Title, string? Description, string? Status, DateTime? DueDate);
public record UpdateTaskRequest(string Title, string? Description, string Status, DateTime? DueDate);
public record TaskDto(Guid Id, string Title, string? Description, string Status, DateTime? DueDate, DateTime CreatedAt, DateTime UpdatedAt);
