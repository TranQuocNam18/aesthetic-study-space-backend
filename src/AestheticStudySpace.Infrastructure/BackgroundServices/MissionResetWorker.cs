using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically cleans up old user-mission rows. Daily/weekly resets are driven by <see cref="MissionPeriodHelper"/> period keys.
/// </summary>
public class MissionResetWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly int RetentionDays = 90;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MissionResetWorker> _logger;

    public MissionResetWorker(IServiceScopeFactory scopeFactory, ILogger<MissionResetWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mission reset worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var userMissionRepository = scope.ServiceProvider.GetRequiredService<IUserMissionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-RetentionDays);
        var removed = await userMissionRepository.DeleteOlderThanAsync(cutoff, cancellationToken);

        if (removed > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Mission reset worker removed {Count} user-mission rows older than {Cutoff}.", removed, cutoff);
        }
    }
}
