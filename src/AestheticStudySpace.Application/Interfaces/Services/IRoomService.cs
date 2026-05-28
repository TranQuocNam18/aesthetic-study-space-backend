using AestheticStudySpace.Application.DTOs.Rooms;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IRoomService
{
    Task<IReadOnlyList<RoomListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> CreateAsync(CreateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> UpdateAsync(Guid id, UpdateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> DuplicateAsync(Guid id, CancellationToken cancellationToken = default);
}
