using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomAssetMappingRepository _mappingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const int FreeRoomLimit = 3;

    public RoomService(
        IRoomRepository roomRepository,
        IRoomAssetMappingRepository mappingRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _mappingRepository = mappingRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    // ─── Admin / Global Rooms ───────────────────────────────────────────────

    public async Task<IReadOnlyList<RoomListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);
        return rooms.Select(r => r.ToListItemDto()).ToList();
    }

    public async Task<RoomDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, includeAssets: false, cancellationToken)
            ?? throw new NotFoundException($"Room '{id}' was not found.");

        var mappings = await _mappingRepository.GetByRoomIdAsync(id, cancellationToken);
        return room.ToDetailDto(mappings);
    }

    public async Task<RoomDetailDto> CreateAsync(CreateRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        var room = new Room
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            BackgroundUrl = request.BackgroundUrl,
            IsPremium = request.IsPremium
        };

        await _roomRepository.AddAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return room.ToDetailDto(Array.Empty<RoomAssetMapping>());
    }

    public async Task<RoomDetailDto> UpdateAsync(Guid id, UpdateRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{id}' was not found.");

        room.Name = request.Name.Trim();
        room.Description = request.Description;
        room.ThumbnailUrl = request.ThumbnailUrl;
        room.BackgroundUrl = request.BackgroundUrl;
        room.IsPremium = request.IsPremium;

        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mappings = await _mappingRepository.GetByRoomIdAsync(id, cancellationToken);
        return room.ToDetailDto(mappings);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{id}' was not found.");

        await _mappingRepository.DeleteByRoomIdAsync(id, cancellationToken);
        await _roomRepository.DeleteAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RoomDetailDto> DuplicateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{id}' was not found.");

        var clone = new Room
        {
            Name = $"{room.Name} (Copy)",
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            BackgroundUrl = room.BackgroundUrl,
            IsPremium = room.IsPremium
        };

        await _roomRepository.AddAsync(clone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mappings = await _mappingRepository.GetByRoomIdAsync(id, cancellationToken);
        foreach (var m in mappings)
        {
            await _mappingRepository.AddAsync(new RoomAssetMapping
            {
                RoomId = clone.Id,
                AssetId = m.AssetId,
                DefaultPositionX = m.DefaultPositionX,
                DefaultPositionY = m.DefaultPositionY,
                DefaultScale = m.DefaultScale,
                DefaultOpacity = m.DefaultOpacity,
                DefaultLayerIndex = m.DefaultLayerIndex
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var cloneMappings = await _mappingRepository.GetByRoomIdAsync(clone.Id, cancellationToken);
        return clone.ToDetailDto(cloneMappings);
    }

    // ─── User Custom Rooms ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<RoomListItemDto>> GetMyRoomsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rooms = await _roomRepository.GetByUserAsync(userId, cancellationToken);
        return rooms.Select(r => r.ToListItemDto()).ToList();
    }

    public async Task<RoomDetailDto> CreateUserRoomAsync(Guid userId, UserCreateRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Room name is required.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("Your account is banned.");

        // Free users may only own up to 3 custom rooms
        if (user.AccountTier == AccountTier.Free)
        {
            var count = await _roomRepository.CountByUserAsync(userId, cancellationToken);
            if (count >= FreeRoomLimit)
                throw new ValidationException($"Free users can only create up to {FreeRoomLimit} custom rooms. Upgrade to Premium for unlimited rooms.");
        }

        var room = new Room
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            BackgroundUrl = request.BackgroundUrl,
            IsPremium = false
        };

        await _roomRepository.AddAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return room.ToDetailDto(Array.Empty<RoomAssetMapping>());
    }

    public async Task<RoomDetailDto> UpdateUserRoomAsync(Guid userId, Guid roomId, UserUpdateRoomRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Room name is required.");

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{roomId}' was not found.");

        if (room.UserId != userId)
            throw new ForbiddenException("You do not have access to this room.");

        room.Name = request.Name.Trim();
        room.Description = request.Description;
        room.ThumbnailUrl = request.ThumbnailUrl;
        room.BackgroundUrl = request.BackgroundUrl;

        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mappings = await _mappingRepository.GetByRoomIdAsync(roomId, cancellationToken);
        return room.ToDetailDto(mappings);
    }

    public async Task DeleteUserRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{roomId}' was not found.");

        if (room.UserId != userId)
            throw new ForbiddenException("You do not have access to this room.");

        await _mappingRepository.DeleteByRoomIdAsync(roomId, cancellationToken);
        await _roomRepository.DeleteAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
