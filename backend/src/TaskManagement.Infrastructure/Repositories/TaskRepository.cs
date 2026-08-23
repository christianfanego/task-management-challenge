using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Ports;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context) => _context = context;

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Tasks.Where(t => t.UserId == userId).ToListAsync(ct);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (task == null) return false;
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
