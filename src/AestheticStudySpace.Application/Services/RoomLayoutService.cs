using AestheticStudySpace.Application.DTOs.RoomLayouts;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class RoomLayoutService : IRoomLayoutService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoomLayoutRepository _roomLayoutRepository;
    private readonly IMediaStorageService _mediaStorage;
    private readonly IUnitOfWork _unitOfWork;

    public RoomLayoutService(
        IUserRepository userRepository,
        IRoomLayoutRepository roomLayoutRepository,
        IMediaStorageService mediaStorage,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roomLayoutRepository = roomLayoutRepository;
        _mediaStorage = mediaStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<RoomLayoutDto>> GetMyLayoutsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var layouts = await _roomLayoutRepository.GetByUserAsync(userId, page, pageSize, cancellationToken);
        return layouts.Select(ToDto).ToList();
    }

    public async Task<RoomLayoutDto> SaveAsync(Guid userId, Guid? layoutId, SaveRoomLayoutRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(request.LayoutJson))
            throw new ValidationException("LayoutJson is required.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        // Free users max 3 layouts; premium unlimited
        if (layoutId is null && user.AccountTier == AccountTier.Free)
        {
            var count = await _roomLayoutRepository.CountByUserAsync(userId, cancellationToken);
            if (count >= 3)
                throw new ValidationException("Free users can only save up to 3 layouts.");
        }

        RoomLayout layout;
        if (layoutId is null)
        {
            layout = new RoomLayout
            {
                UserId = userId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                RoomId = request.RoomId,
                LayoutJson = request.LayoutJson
            };

            if (!string.IsNullOrWhiteSpace(request.ThumbnailBase64Png))
                layout.ThumbnailUrl = await _mediaStorage.UploadBase64ImageAsync(request.ThumbnailBase64Png, "ass/thumbnails", cancellationToken);

            await _roomLayoutRepository.AddAsync(layout, cancellationToken);
        }
        else
        {
            layout = await _roomLayoutRepository.GetByIdAsync(layoutId.Value, cancellationToken)
                ?? throw new NotFoundException("Layout not found.");

            if (layout.UserId != userId)
                throw new ForbiddenException("You do not have access to this layout.");

            layout.Name = request.Name.Trim();
            layout.Description = request.Description?.Trim();
            layout.RoomId = request.RoomId;
            layout.LayoutJson = request.LayoutJson;

            if (!string.IsNullOrWhiteSpace(request.ThumbnailBase64Png))
                layout.ThumbnailUrl = await _mediaStorage.UploadBase64ImageAsync(request.ThumbnailBase64Png, "ass/thumbnails", cancellationToken);

            await _roomLayoutRepository.UpdateAsync(layout, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(layout);
    }

    public async Task DeleteAsync(Guid userId, Guid layoutId, CancellationToken cancellationToken = default)
    {
        var layout = await _roomLayoutRepository.GetByIdAsync(layoutId, cancellationToken)
            ?? throw new NotFoundException("Layout not found.");

        if (layout.UserId != userId)
            throw new ForbiddenException("You do not have access to this layout.");

        await _roomLayoutRepository.DeleteAsync(layout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RoomLayoutDto> DuplicateAsync(Guid userId, Guid layoutId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountTier == AccountTier.Free)
        {
            var count = await _roomLayoutRepository.CountByUserAsync(userId, cancellationToken);
            if (count >= 3)
                throw new ValidationException("Free users can only save up to 3 layouts.");
        }

        var layout = await _roomLayoutRepository.GetByIdAsync(layoutId, cancellationToken)
            ?? throw new NotFoundException("Layout not found.");

        if (layout.UserId != userId)
            throw new ForbiddenException("You do not have access to this layout.");

        var clone = new RoomLayout
        {
            UserId = userId,
            Name = $"{layout.Name} (Copy)",
            Description = layout.Description,
            RoomId = layout.RoomId,
            LayoutJson = layout.LayoutJson,
            ThumbnailUrl = layout.ThumbnailUrl
        };

        await _roomLayoutRepository.AddAsync(clone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(clone);
    }

    private static RoomLayoutDto ToDto(RoomLayout layout) =>
        new(layout.Id, layout.Name, layout.Description, layout.RoomId, layout.LayoutJson, layout.ThumbnailUrl, layout.CreatedAt, layout.UpdatedAt);
}

