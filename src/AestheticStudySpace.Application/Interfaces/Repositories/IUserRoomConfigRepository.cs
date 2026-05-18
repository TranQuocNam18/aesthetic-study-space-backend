using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IUserRoomConfigRepository
{
    Task<UserRoomConfig?> GetByUserAndRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRoomConfig>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserRoomConfig config, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserRoomConfig config, CancellationToken cancellationToken = default);
}
