using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IRoomLayoutRepository
{
    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomLayout>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<RoomLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RoomLayout layout, CancellationToken cancellationToken = default);
    Task UpdateAsync(RoomLayout layout, CancellationToken cancellationToken = default);
    Task DeleteAsync(RoomLayout layout, CancellationToken cancellationToken = default);
}

