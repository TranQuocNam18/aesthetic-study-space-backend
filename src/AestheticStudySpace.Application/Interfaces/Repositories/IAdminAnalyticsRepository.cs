using AestheticStudySpace.Application.DTOs.Admin;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IAdminAnalyticsRepository
{
    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminDateCountDto>> GetUserGrowthAsync(int days, CancellationToken cancellationToken = default);
    Task<AdminFeatureUsageDto> GetFeatureUsageAsync(CancellationToken cancellationToken = default);
}

