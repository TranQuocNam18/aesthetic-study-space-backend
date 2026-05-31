using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Room>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Room?> GetByIdAsync(Guid id, bool includeAssets = false, CancellationToken cancellationToken = default);
    Task AddAsync(Room room, CancellationToken cancellationToken = default);
    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);
    Task DeleteAsync(Room room, CancellationToken cancellationToken = default);
}
