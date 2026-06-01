using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminUserDto> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task BanUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task UnbanUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUserDto> UpdateUserTierAsync(Guid id, string tier, CancellationToken cancellationToken = default);

    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminDateCountDto>> GetUserGrowthAsync(int days, CancellationToken cancellationToken = default);
    Task<AdminFeatureUsageDto> GetFeatureUsageAsync(CancellationToken cancellationToken = default);
}

