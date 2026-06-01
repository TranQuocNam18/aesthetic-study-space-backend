using AestheticStudySpace.Application.DTOs.RoomLayouts;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IRoomLayoutService
{
    Task<IReadOnlyList<RoomLayoutDto>> GetMyLayoutsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<RoomLayoutDto> SaveAsync(Guid userId, Guid? layoutId, SaveRoomLayoutRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid layoutId, CancellationToken cancellationToken = default);
    Task<RoomLayoutDto> DuplicateAsync(Guid userId, Guid layoutId, CancellationToken cancellationToken = default);
}

