using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminUserDto> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task BanUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task UnbanUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUserDto> UpdateUserTierAsync(Guid id, string tier, CancellationToken cancellationToken = default);
    Task AddCoinsToUserAsync(Guid id, int amount, CancellationToken cancellationToken = default);

    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminDateCountDto>> GetUserGrowthAsync(int days, CancellationToken cancellationToken = default);
    Task<AdminFeatureUsageDto> GetFeatureUsageAsync(CancellationToken cancellationToken = default);
    Task<AdminRevenueSummaryDto> GetRevenueSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminRevenueTrendDto>> GetRevenueTrendAsync(int days, CancellationToken cancellationToken = default);

    Task<PagedResult<AdminPaymentTransactionDto>> GetPaymentsAsync(
        string? search,
        PaymentProvider? provider,
        PaymentStatus? status,
        PaymentPurpose? purpose,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminPaymentTransactionDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task ManualFulfillPaymentAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeletePaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeletePaymentsByProviderAsync(PaymentProvider provider, CancellationToken cancellationToken = default);
}

