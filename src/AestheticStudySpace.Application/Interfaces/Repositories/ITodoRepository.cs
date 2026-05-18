using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface ITodoRepository
{
    Task<IReadOnlyList<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Todo todo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default);
}
