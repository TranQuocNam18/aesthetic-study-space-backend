using AestheticStudySpace.Application.DTOs.Rooms;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IRoomService
{
    // Admin-facing (global rooms)
    Task<IReadOnlyList<RoomListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> CreateAsync(CreateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> UpdateAsync(Guid id, UpdateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> DuplicateAsync(Guid id, CancellationToken cancellationToken = default);

    // User-facing (custom rooms with tier limit)
    Task<IReadOnlyList<RoomListItemDto>> GetMyRoomsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> CreateUserRoomAsync(Guid userId, UserCreateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> UpdateUserRoomAsync(Guid userId, Guid roomId, UserUpdateRoomRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteUserRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default);
}
