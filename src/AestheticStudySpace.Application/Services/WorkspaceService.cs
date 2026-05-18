using System.Text.Json;
using AestheticStudySpace.Application.DTOs.Workspace;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUserRoomConfigRepository _configRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceService(
        IUserRoomConfigRepository configRepository,
        IRoomRepository roomRepository,
        IUnitOfWork unitOfWork)
    {
        _configRepository = configRepository;
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkspaceConfigDto>> GetMyWorkspaceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetByUserIdAsync(userId, cancellationToken);
        return configs.Select(c => new WorkspaceConfigDto(
            c.Id,
            c.RoomId,
            c.Room.Name,
            c.JsonConfig,
            c.UpdatedAt)).ToList();
    }

    public async Task<WorkspaceConfigDto> SaveAsync(Guid userId, SaveWorkspaceRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateJsonConfig(request.JsonConfig);

        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Room '{request.RoomId}' was not found.");

        var existing = await _configRepository.GetByUserAndRoomAsync(userId, request.RoomId, cancellationToken);

        if (existing is null)
        {
            var config = new UserRoomConfig
            {
                UserId = userId,
                RoomId = request.RoomId,
                JsonConfig = request.JsonConfig,
                UpdatedAt = DateTime.UtcNow
            };
            await _configRepository.AddAsync(config, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new WorkspaceConfigDto(config.Id, room.Id, room.Name, config.JsonConfig, config.UpdatedAt);
        }

        existing.JsonConfig = request.JsonConfig;
        existing.UpdatedAt = DateTime.UtcNow;
        await _configRepository.UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkspaceConfigDto(existing.Id, room.Id, room.Name, existing.JsonConfig, existing.UpdatedAt);
    }

    private static void ValidateJsonConfig(string jsonConfig)
    {
        if (string.IsNullOrWhiteSpace(jsonConfig))
            throw new ValidationException("JsonConfig cannot be empty.");

        try
        {
            using var doc = JsonDocument.Parse(jsonConfig);
        }
        catch (JsonException)
        {
            throw new ValidationException("JsonConfig must be valid JSON.");
        }
    }
}
