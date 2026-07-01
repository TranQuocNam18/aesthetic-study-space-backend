using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.BackgroundServices;

public class RetentionEmailWorker : BackgroundService
{
    // Run scan every 12 hours
    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetentionEmailWorker> _logger;

    public RetentionEmailWorker(IServiceScopeFactory scopeFactory, ILogger<RetentionEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay 2 minutes to let the app start up and migrate DB smoothly
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        _logger.LogInformation("Retention Email Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRetentionEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Retention Email Worker.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SendRetentionEmailsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var retentionEmailService = scope.ServiceProvider.GetRequiredService<IRetentionEmailService>();
        await retentionEmailService.SendRetentionEmailsAsync(cancellationToken);
    }
}
