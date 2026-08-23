namespace TaskManagement.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UserId { get; set; }

    public static TaskItem Create(string title, string? description, DateTime? dueDate, Guid userId)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = NormalizeDescription(description),
            Status = TaskStatus.Pending,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };
        return task;
    }

    public void Update(string title, string? description, TaskStatus status, DateTime? dueDate)
    {
        Title = title.Trim();
        Description = NormalizeDescription(description);
        Status = status;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;
        return description.Trim();
    }
}
