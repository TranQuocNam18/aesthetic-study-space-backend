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
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var list = new List<MissionWithProgressDto>();
        foreach (var m in missions)
        {
            UserMission? um;
            DateOnly period;

            if (IsRollingOrStreak(m.Frequency))
            {
                um = progressRows
                    .Where(x => x.MissionId == m.Id)
                    .OrderByDescending(x => x.PeriodDate)
                    .FirstOrDefault();

                if (um is not null && !MissionPeriodHelper.IsPeriodValid(m.Frequency, um.PeriodDate, today))
                {
                    um = null;
                }
                period = um?.PeriodDate ?? today;
            }
            else
            {
                period = MissionPeriodHelper.GetPeriodDate(m.Frequency);
                um = progressRows.FirstOrDefault(x => x.MissionId == m.Id && x.PeriodDate == period);
            }

            list.Add(ToProgressDto(m, um, period));
        }

        return list;
    }

    public async Task<MissionWithProgressDto> GetByIdForUserAsync(Guid userId, Guid missionId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        UserMission? um;
        DateOnly period;

        if (IsRollingOrStreak(mission.Frequency))
        {
            um = await _userMissionRepository.GetLatestForMissionAsync(userId, missionId, cancellationToken);
            if (um is not null && !MissionPeriodHelper.IsPeriodValid(mission.Frequency, um.PeriodDate, today))
            {
                um = null;
            }
            period = um?.PeriodDate ?? today;
        }
        else
        {
            period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
            um = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken);
        }

        return ToProgressDto(mission, um, period);
    }

    public async Task<UserMissionDto> IncrementAsync(Guid userId, Guid missionId, int delta, CancellationToken cancellationToken = default)
    {
        if (delta <= 0)
            throw new ValidationException("Delta must be positive.");

        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        if (!mission.IsActive)
            throw new ValidationException("Mission is not active.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var frequency = mission.Frequency.Trim().ToLowerInvariant();
        UserMission? userMission = null;
        var isNew = false;
        DateOnly period;

        if (frequency == "daily_login_streak")
        {
            userMission = await _userMissionRepository.GetLatestForMissionAsync(userId, missionId, cancellationToken);
            if (userMission is null || (userMission.IsCompleted && userMission.PeriodDate < today))
            {
                period = today;
                userMission = new UserMission
                {
                    UserId = userId,
                    MissionId = missionId,
                    PeriodDate = today,
                    ProgressValue = 1
                };
                isNew = true;
                await _userMissionRepository.AddAsync(userMission, cancellationToken);
            }
            else if (userMission.IsDeleted)
            {
                userMission.IsDeleted = false;
                userMission.DeletedAt = null;
                userMission.DeletedBy = null;
                userMission.PeriodDate = today;
                userMission.ProgressValue = 1;
                userMission.IsCompleted = false;
                userMission.CompletedAt = null;
                userMission.ClaimedAt = null;
                userMission.UpdatedAt = DateTime.UtcNow;
                await _userMissionRepository.UpdateAsync(userMission, cancellationToken);
            }
            else if (userMission.PeriodDate == today)
            {
                // Already logged in today — streak unchanged
                return ToDto(userMission);
            }
            else if (userMission.PeriodDate == today.AddDays(-1))
            {
                // Consecutive day — continue streak
                userMission.PeriodDate = today;
                userMission.ProgressValue += 1;
                userMission.UpdatedAt = DateTime.UtcNow;
                await _userMissionRepository.UpdateAsync(userMission, cancellationToken);
            }
            else
            {
                // Missed at least 1 day — RESET STREAK back to day 1
                userMission.PeriodDate = today;
                userMission.ProgressValue = 1;
                userMission.IsCompleted = false;
                userMission.CompletedAt = null;
                userMission.ClaimedAt = null;
                userMission.UpdatedAt = DateTime.UtcNow;
                await _userMissionRepository.UpdateAsync(userMission, cancellationToken);
            }
        }
        else if (frequency == "rolling_weekly")
        {
            userMission = await _userMissionRepository.GetLatestForMissionAsync(userId, missionId, cancellationToken);
            if (userMission is null || !MissionPeriodHelper.IsPeriodValid("rolling_weekly", userMission.PeriodDate, today))
            {
                period = today;
                userMission = new UserMission
                {
                    UserId = userId,
                    MissionId = missionId,
                    PeriodDate = today,
                    ProgressValue = 0
                };
                isNew = true;
                await _userMissionRepository.AddAsync(userMission, cancellationToken);
            }
            else
            {
                period = userMission.PeriodDate;
            }

            if (!userMission.IsCompleted)
            {
                userMission.ProgressValue += delta;
            }
        }
        else
        {
            period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
            userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken);
            isNew = userMission is null;

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
                userMission.IsDeleted = false;
                userMission.DeletedAt = null;
                userMission.DeletedBy = null;
                userMission.ProgressValue = 0;
                userMission.IsCompleted = false;
                userMission.CompletedAt = null;
                userMission.ClaimedAt = null;
                userMission.UpdatedAt = DateTime.UtcNow;
                await _userMissionRepository.UpdateAsync(userMission, cancellationToken);
            }

            if (!userMission.IsCompleted)
            {
                userMission.ProgressValue += delta;
            }
        }

        if (mission.TargetValue is null || userMission.ProgressValue >= mission.TargetValue.Value)
        {
            userMission.IsCompleted = true;
            userMission.CompletedAt = DateTime.UtcNow;
        }

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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        UserMission? userMission;

        if (IsRollingOrStreak(mission.Frequency))
        {
            userMission = await _userMissionRepository.GetLatestForMissionAsync(userId, missionId, cancellationToken);
            if (userMission is not null && !MissionPeriodHelper.IsPeriodValid(mission.Frequency, userMission.PeriodDate, today))
            {
                userMission = null;
            }
        }
        else
        {
            var period = MissionPeriodHelper.GetPeriodDate(mission.Frequency);
            userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken);
        }

        if (userMission is null)
            throw new ValidationException("Mission progress not found for current period.");

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

    private static bool IsRollingOrStreak(string? frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency)) return false;
        var f = frequency.Trim().ToLowerInvariant();
        return f is "rolling_weekly" or "daily_login_streak";
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
