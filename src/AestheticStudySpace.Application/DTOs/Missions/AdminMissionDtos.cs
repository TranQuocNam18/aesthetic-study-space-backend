namespace AestheticStudySpace.Application.DTOs.Missions;

public record AdminMissionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int RewardCoins,
    string TriggerKey,
    int? TargetValue,
    string Frequency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateMissionRequestDto(
    string Code,
    string Name,
    string? Description,
    int RewardCoins,
    string TriggerKey,
    int? TargetValue,
    string Frequency = "daily",
    bool IsActive = true);

public record UpdateMissionRequestDto(
    string Code,
    string Name,
    string? Description,
    int RewardCoins,
    string TriggerKey,
    int? TargetValue,
    string Frequency,
    bool IsActive);

public record MissionWithProgressDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int RewardCoins,
    string TriggerKey,
    int? TargetValue,
    string Frequency,
    int ProgressValue,
    bool IsCompleted,
    bool IsClaimed,
    DateOnly PeriodDate,
    DateTime? CompletedAt,
    DateTime? ClaimedAt);

public record MissionMetadataOptionsDto(
    IReadOnlyList<string> AllowedTriggerKeys,
    IReadOnlyList<string> AllowedFrequencies);

