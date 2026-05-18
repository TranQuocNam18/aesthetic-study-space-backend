using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomAssetMappingRepository _mappingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IRoomRepository roomRepository, IRoomAssetMappingRepository mappingRepository, IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _mappingRepository = mappingRepository;
        _unitOfWork = unitOfWork;
    }

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
}
