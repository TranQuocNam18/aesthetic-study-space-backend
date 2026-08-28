namespace AestheticStudySpace.Application.DTOs.LuckyDraw;

public record LuckyDrawStatusDto(
    int RemainingDrawsToday,
    int MaxDrawsToday,
    bool CanSpin,
    bool IsPremium,
    IReadOnlyList<LuckyDrawHistoryItemDto> DrawHistoryToday);

public record LuckyDrawHistoryItemDto(
    Guid Id,
    int RewardCoins,
    string RewardDescription,
    DateTime CreatedAt);

public record LuckyDrawResultDto(
    int RewardCoins,
    string RewardDescription,
    int RemainingDrawsToday,
    int NewCoinsBalance);
