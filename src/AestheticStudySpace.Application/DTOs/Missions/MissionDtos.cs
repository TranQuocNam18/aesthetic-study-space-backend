namespace AestheticStudySpace.Application.DTOs.Missions;

public record MissionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int RewardCoins,
    string TriggerKey,
    int? TargetValue,
    string Frequency);

public record UserMissionDto(
    Guid MissionId,
    int ProgressValue,
    bool IsCompleted,
    DateOnly PeriodDate,
    DateTime? CompletedAt,
    DateTime? ClaimedAt,
    int CoinsEarned = 0,
    double Multiplier = 1.0);

