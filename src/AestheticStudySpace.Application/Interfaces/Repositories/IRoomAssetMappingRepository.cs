using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IRoomAssetMappingRepository
{
    Task<IReadOnlyList<RoomAssetMapping>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task AddAsync(RoomAssetMapping mapping, CancellationToken cancellationToken = default);
    Task DeleteByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
}
