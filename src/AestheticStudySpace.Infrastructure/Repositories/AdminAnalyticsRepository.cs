using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class AdminAnalyticsRepository : IAdminAnalyticsRepository
{
    private readonly AppDbContext _context;

    public AdminAnalyticsRepository(AppDbContext context) => _context = context;

    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var activeUsersToday = await _context.Users.CountAsync(u => u.LastLoginAt != null && u.LastLoginAt >= today && u.LastLoginAt < tomorrow, cancellationToken);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart, cancellationToken);

        var premiumUsers = await _context.Users.CountAsync(u => u.AccountTier == AccountTier.Premium, cancellationToken);

        var totalRevenue = await _context.PaymentTransactions
            .Where(t => t.Status == PaymentStatus.Succeeded)
            .SumAsync(t => (long?)t.Amount, cancellationToken) ?? 0;

        return new AdminOverviewDto(totalUsers, activeUsersToday, newUsersThisMonth, premiumUsers, totalRevenue);
    }

    public async Task<IReadOnlyList<AdminDateCountDto>> GetUserGrowthAsync(int days, CancellationToken cancellationToken = default)
    {
        days = days switch { <= 0 => 7, > 365 => 365, _ => days };
        var from = DateTime.UtcNow.Date.AddDays(-days + 1);
        var to = DateTime.UtcNow.AddDays(1).Date;

        var data = await _context.Users.AsNoTracking()
            .Where(u => u.CreatedAt >= from && u.CreatedAt < to)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return data.Select(x => new AdminDateCountDto(DateOnly.FromDateTime(x.Date), x.Count)).ToList();
    }

    public async Task<AdminFeatureUsageDto> GetFeatureUsageAsync(CancellationToken cancellationToken = default)
    {
        var pomodoro = await _context.PomodoroSessions.CountAsync(p => p.EndTime != null, cancellationToken);
        var todosCompleted = await _context.Todos.CountAsync(t => t.IsCompleted, cancellationToken);
        var paymentsSucceeded = await _context.PaymentTransactions.CountAsync(t => t.Status == PaymentStatus.Succeeded, cancellationToken);

        var roomsVisited = await _context.ActivityLogs.CountAsync(a => a.Action == "room_visit", cancellationToken);
        return new AdminFeatureUsageDto(pomodoro, todosCompleted, roomsVisited, paymentsSucceeded);
    }
}

