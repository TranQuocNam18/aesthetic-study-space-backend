using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Missions;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class AdminMissionService : IAdminMissionService
{
    private static readonly HashSet<string> AllowedFrequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "daily", "weekly", "once"
    };

    private static readonly HashSet<string> AllowedTriggerKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "daily_login", "pomodoro_complete", "study_minutes"
    };

    private readonly IMissionRepository _missionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminMissionService(IMissionRepository missionRepository, IUnitOfWork unitOfWork)
    {
        _missionRepository = missionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdminMissionDto>> GetMissionsAsync(
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _missionRepository.CountAllAsync(includeInactive, cancellationToken);
        var missions = await _missionRepository.GetAllAsync(includeInactive, page, pageSize, cancellationToken);

        return new PagedResult<AdminMissionDto>
        {
            Items = missions.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminMissionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");
        return ToAdminDto(mission);
    }

    public async Task<AdminMissionDto> CreateAsync(CreateMissionRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Code, request.Name, request.RewardCoins, request.TriggerKey, request.Frequency, request.TargetValue);

        if (await _missionRepository.GetByCodeAsync(request.Code.Trim(), cancellationToken) is not null)
            throw new ValidationException("Mission code already exists.");

        var mission = new Mission
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            RewardCoins = request.RewardCoins,
            TriggerKey = request.TriggerKey.Trim(),
            TargetValue = request.TargetValue,
            Frequency = request.Frequency.Trim().ToLowerInvariant(),
            IsActive = request.IsActive
        };

        await _missionRepository.AddAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(mission);
    }

    public async Task<AdminMissionDto> UpdateAsync(Guid id, UpdateMissionRequestDto request, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        ValidateRequest(request.Code, request.Name, request.RewardCoins, request.TriggerKey, request.Frequency, request.TargetValue);

        var existingCode = await _missionRepository.GetByCodeAsync(request.Code.Trim(), cancellationToken);
        if (existingCode is not null && existingCode.Id != id)
            throw new ValidationException("Mission code already exists.");

        mission.Code = request.Code.Trim();
        mission.Name = request.Name.Trim();
        mission.Description = request.Description?.Trim();
        mission.RewardCoins = request.RewardCoins;
        mission.TriggerKey = request.TriggerKey.Trim();
        mission.TargetValue = request.TargetValue;
        mission.Frequency = request.Frequency.Trim().ToLowerInvariant();
        mission.IsActive = request.IsActive;
        mission.UpdatedAt = DateTime.UtcNow;

        await _missionRepository.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(mission);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        mission.IsActive = false;
        mission.IsDeleted = true;
        mission.DeletedAt = DateTime.UtcNow;
        mission.UpdatedAt = DateTime.UtcNow;

        await _missionRepository.UpdateAsync(mission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(string code, string name, int rewardCoins, string triggerKey, string frequency, int? targetValue)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(triggerKey))
            throw new ValidationException("TriggerKey is required.");

        if (!AllowedTriggerKeys.Contains(triggerKey.Trim()))
            throw new ValidationException("TriggerKey must be daily_login, pomodoro_complete, or study_minutes.");

        if (rewardCoins <= 0)
            throw new ValidationException("RewardCoins must be positive.");

        if (!AllowedFrequencies.Contains(frequency))
            throw new ValidationException("Frequency must be daily, weekly, or once.");

        if (targetValue is <= 0)
            throw new ValidationException("TargetValue must be positive when set.");
    }

    private static AdminMissionDto ToAdminDto(Mission m) =>
        new(m.Id, m.Code, m.Name, m.Description, m.RewardCoins, m.TriggerKey, m.TargetValue, m.Frequency, m.IsActive, m.CreatedAt, m.UpdatedAt);
}
