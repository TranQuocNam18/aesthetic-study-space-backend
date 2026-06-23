using AestheticStudySpace.Domain.Enums;

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

/// <summary>Tổng doanh thu phân loại theo mục đích thanh toán.</summary>
public record AdminRevenueSummaryDto(
    long TotalRevenueVnd,
    long SubscriptionRevenueVnd,
    long CoinPackRevenueVnd,
    long AssetRevenueVnd,
    int TotalTransactions);

/// <summary>Doanh thu theo từng ngày (dùng cho biểu đồ xu hướng).</summary>
public record AdminRevenueTrendDto(DateOnly Date, long AmountVnd, int Transactions);

