using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAdminStoreService
{
    Task<PagedResult<AdminStoreItemDto>> GetItemsAsync(StoreCategory? category, StoreThemeSource? themeSource, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> CreateAsync(CreateStoreItemRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> UpdateAsync(Guid id, UpdateStoreItemRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> PatchAsync(Guid id, PatchStoreItemRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // ── Creator Buyout Transaction & Pricing Pool Workflow ─────────────────────
    Task<PagedResult<AdminStoreItemDto>> GetPendingTransactionsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> ApproveTransactionAsync(Guid id, AdminApproveTransactionDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> RejectTransactionAsync(Guid id, RejectThemeRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminStoreItemDto>> GetPurchasedPendingPricingAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> PriceAndPublishAsync(Guid id, AdminPriceAndPublishDto request, CancellationToken cancellationToken = default);
}
