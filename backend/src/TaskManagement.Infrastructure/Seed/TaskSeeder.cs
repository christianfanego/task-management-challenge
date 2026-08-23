using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Seed;

public static class TaskSeeder
{
    public static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid Task1Id = Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid Task2Id = Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid Task3Id = Guid.Parse("00000000-0000-0000-0000-000000000103");

    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any()) return;

        context.Users.Add(new User
        {
            Id = DemoUserId,
            Email = "demo@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("DemoPass123!"),
            CreatedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
        });

        context.Tasks.AddRange(
            new TaskItem
            {
                Id = Task1Id,
                Title = "Prepare weekly review",
                Description = "Summarize completed work",
                Status = TaskStatus.Pending,
                DueDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                UserId = DemoUserId
            },
            new TaskItem
            {
                Id = Task2Id,
                Title = "Ship task API",
                Description = "Verify ownership and validation",
                Status = TaskStatus.InProgress,
                DueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc),
                UserId = DemoUserId
            },
            new TaskItem
            {
                Id = Task3Id,
                Title = "Archive January notes",
                Description = null,
                Status = TaskStatus.Completed,
                DueDate = null,
                CreatedAt = new DateTime(2026, 1, 3, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 3, 9, 0, 0, DateTimeKind.Utc),
                UserId = DemoUserId
            }
        );

        context.SaveChanges();
    }
}
