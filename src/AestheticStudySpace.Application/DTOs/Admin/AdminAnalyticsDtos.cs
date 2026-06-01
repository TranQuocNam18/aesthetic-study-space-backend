namespace AestheticStudySpace.Application.DTOs.Admin;

public record AdminOverviewDto(
    int TotalUsers,
    int ActiveUsersToday,
    int NewUsersThisMonth,
    int TotalPremiumUsers,
    long TotalRevenueVnd);

public record AdminDateCountDto(DateOnly Date, int Count);

public record AdminFeatureUsageDto(
    int PomodoroSessions,
    int TodosCompleted,
    int RoomsVisited,
    int PaymentsSucceeded);

