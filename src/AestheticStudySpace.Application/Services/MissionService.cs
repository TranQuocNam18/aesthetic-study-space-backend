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

    public async Task<UserMissionDto> IncrementAsync(Guid userId, Guid missionId, int delta, CancellationToken cancellationToken = default)
    {
        if (delta <= 0)
            throw new ValidationException("Delta must be positive.");

        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        if (!mission.IsActive)
            throw new ValidationException("Mission is not active.");

        var period = DateOnly.FromDateTime(DateTime.UtcNow);
        var userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken);

        if (userMission is null)
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

        if (userMission.IsCompleted)
            return ToDto(userMission);

        userMission.ProgressValue += delta;

        if (mission.TargetValue is null || userMission.ProgressValue >= mission.TargetValue.Value)
        {
            userMission.IsCompleted = true;
            userMission.CompletedAt = DateTime.UtcNow;
        }

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
            await IncrementAsync(userId, mission.Id, delta, cancellationToken);
        }
    }

    public async Task<UserMissionDto> ClaimAsync(Guid userId, Guid missionId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        var period = DateOnly.FromDateTime(DateTime.UtcNow);
        var userMission = await _userMissionRepository.GetForPeriodAsync(userId, missionId, period, cancellationToken)
            ?? throw new ValidationException("Mission progress not found for current period.");

        if (!userMission.IsCompleted)
            throw new ValidationException("Mission not completed.");

        if (userMission.ClaimedAt is not null)
            throw new ValidationException("Mission already claimed.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.CoinsBalance += mission.RewardCoins;
        await _userRepository.UpdateAsync(user, cancellationToken);

        userMission.ClaimedAt = DateTime.UtcNow;
        await _userMissionRepository.UpdateAsync(userMission, cancellationToken);

        var tx = new CoinTransaction
        {
            UserId = userId,
            Type = CoinTransactionType.Earned,
            Amount = mission.RewardCoins,
            Reason = $"Mission:{mission.Code}",
            RelatedMissionId = mission.Id
        };
        await _coinTransactionRepository.AddAsync(tx, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(userMission);
    }

    private static MissionDto ToDto(Mission m) =>
        new(m.Id, m.Code, m.Name, m.Description, m.RewardCoins, m.TriggerKey, m.TargetValue, m.Frequency);

    private static UserMissionDto ToDto(UserMission um) =>
        new(um.MissionId, um.ProgressValue, um.IsCompleted, um.PeriodDate, um.CompletedAt, um.ClaimedAt);
}

