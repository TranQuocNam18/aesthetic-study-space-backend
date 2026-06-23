using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.BackgroundServices;

public class SubscriptionExpirationWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpirationWorker> _logger;

    public SubscriptionExpirationWorker(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpirationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay 1 minute to avoid overlapping with initial database updates/migration on startup
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpirationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription expiration worker encountered an error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessExpirationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var usersToDowngrade = await context.Users
            .Where(u => u.AccountTier == AccountTier.Premium)
            .Where(u => !context.Subscriptions.Any(s => s.UserId == u.Id && s.IsActive && s.EndsAt > now))
            .ToListAsync(cancellationToken);

        if (usersToDowngrade.Any())
        {
            foreach (var user in usersToDowngrade)
            {
                user.AccountTier = AccountTier.Free;
                _logger.LogInformation("Downgraded user {UserId} ({Username}) to Free tier due to subscription expiration.", user.Id, user.Username);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
