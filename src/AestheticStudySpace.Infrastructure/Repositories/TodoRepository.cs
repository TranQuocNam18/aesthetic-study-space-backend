using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Todos
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Todo todo, CancellationToken cancellationToken = default) =>
        await _context.Todos.AddAsync(todo, cancellationToken);

    public Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Todos.Update(todo);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Todos.Remove(todo);
        return Task.CompletedTask;
    }
}
