using AestheticStudySpace.Application.DTOs.Payments;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface ISubscriptionService
{
    Task<object> UpgradeAsync(Guid userId, SubscriptionUpgradeRequestDto request, CancellationToken cancellationToken = default);
    Task<object> ActivateTrialAsync(Guid userId, CancellationToken cancellationToken = default);
}

