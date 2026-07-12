using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Missions;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IUserMissionRepository _userMissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MissionService(
        IMissionRepository missionRepository,
        IUserMissionRepository userMissionRepository,
        IUserRepository userRepository,
        ICoinTransactionRepository coinTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _missionRepository = missionRepository;
        _userMissionRepository = userMissionRepository;
        _userRepository = userRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MissionDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var missions = await _missionRepository.GetActiveAsync(cancellationToken);
        return missions.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<MissionWithProgressDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var missions = await _missionRepository.GetActiveAsync(cancellationToken);
        var progressRows = await _userMissionRepository.GetByUserAsync(userId, cancellationToken);

        return missions.Select(m =>
        {
            var period = MissionPeriodHelper.GetPeriodDate(m.Frequency);
            var um = progressRows.FirstOrDefault(x => x.MissionId == m.Id && x.PeriodDate == period);
            return ToProgressDto(m, um, period);
        }).ToList();
    }

    public async Task<UserMissionDto> IncrementAsync(Guid userId, Guid missionId, int delta, CancellationToken cancellationToken = default)
    {
        if (delta <= 0)
            throw new ValidationException("Delta must be positive.");

        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        if (!mission.IsActive)
            throw new ValidationException("Mission is not active.");

        var period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
        var userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken);
        var isNew = userMission is null;

        if (isNew)
        {
            userMission = new UserMission
            {
                UserId = userId,
                MissionId = missionId,
                PeriodDate = period,
                ProgressValue = 0
            };
            await _userMissionRepository.AddAsync(userMission, cancellationToken);
        }
        else if (userMission!.IsDeleted)
        {
            // Restore soft-deleted mission progress for the current period
            userMission.IsDeleted = false;
            userMission.DeletedAt = null;
            userMission.DeletedBy = null;
            userMission.ProgressValue = 0;
            userMission.IsCompleted = false;
            userMission.CompletedAt = null;
            userMission.ClaimedAt = null;
            userMission.UpdatedAt = DateTime.UtcNow;
            
            // Explicitly track the update since it was previously detached or soft-deleted
            await _userMissionRepository.UpdateAsync(userMission, cancellationToken);
        }

        if (userMission.IsCompleted)
            return ToDto(userMission);

        userMission.ProgressValue += delta;

        if (mission.TargetValue is null || userMission.ProgressValue >= mission.TargetValue.Value)
        {
            userMission.IsCompleted = true;
            userMission.CompletedAt = DateTime.UtcNow;
        }

        // Do not call Update on a newly Added entity — EF would switch to Modified and issue UPDATE instead of INSERT.
        if (!isNew && !userMission.IsDeleted)
            await _userMissionRepository.UpdateAsync(userMission, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(userMission);
    }

    public async Task IncrementByTriggerKeyAsync(Guid userId, string triggerKey, int delta, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(triggerKey) || delta <= 0)
            return;

        var missions = await _missionRepository.GetActiveByTriggerKeyAsync(triggerKey.Trim(), cancellationToken);
        foreach (var mission in missions)
        {
            if (string.Equals(mission.TriggerKey, "long_focus_session", StringComparison.OrdinalIgnoreCase))
            {
                if (mission.TargetValue.HasValue && delta >= mission.TargetValue.Value)
                {
                    var period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
                    var um = await _userMissionRepository.GetForPeriodAsync(userId, mission.Id, period, cancellationToken);
                    var currentProgress = um?.ProgressValue ?? 0;
                    if (currentProgress < mission.TargetValue.Value)
                    {
                        var needed = mission.TargetValue.Value - currentProgress;
                        await IncrementAsync(userId, mission.Id, needed, cancellationToken);
                    }
                }
            }
            else
            {
                await IncrementAsync(userId, mission.Id, delta, cancellationToken);
            }
        }
    }

    public async Task<UserMissionDto> ClaimAsync(Guid userId, Guid missionId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        var period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
        var userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken)
            ?? throw new ValidationException("Mission progress not found for current period.");

        if (!userMission.IsCompleted)
            throw new ValidationException("Mission not completed.");

        if (userMission.ClaimedAt is not null)
            throw new ValidationException("Mission already claimed.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("User is banned.");

        user.CoinsBalance += mission.RewardCoins;
        await _userRepository.UpdateAsync(user, cancellationToken);

        userMission.ClaimedAt = DateTime.UtcNow;
        await _userMissionRepository.UpdateAsync(userMission, cancellationToken);

        await _coinTransactionRepository.AddAsync(new CoinTransaction
        {
            UserId = userId,
            Type = CoinTransactionType.Earned,
            Amount = mission.RewardCoins,
            Reason = $"Mission:{mission.Code}",
            RelatedMissionId = mission.Id
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(userMission);
    }

    private static MissionDto ToDto(Mission m) =>
        new(m.Id, m.Code, m.Name, m.Description, m.RewardCoins, m.TriggerKey, m.TargetValue, m.Frequency);

    private static MissionWithProgressDto ToProgressDto(Mission m, UserMission? um, DateOnly period) =>
        new(
            m.Id,
            m.Code,
            m.Name,
            m.Description,
            m.RewardCoins,
            m.TriggerKey,
            m.TargetValue,
            m.Frequency,
            um?.ProgressValue ?? 0,
            um?.IsCompleted ?? false,
            um?.ClaimedAt is not null,
            um?.PeriodDate ?? period,
            um?.CompletedAt,
            um?.ClaimedAt);

    private static UserMissionDto ToDto(UserMission um) =>
        new(um.MissionId, um.ProgressValue, um.IsCompleted, um.PeriodDate, um.CompletedAt, um.ClaimedAt);
}
