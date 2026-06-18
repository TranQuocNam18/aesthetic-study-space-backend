using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public enum StoreCatalogScope
{
    All = 0,
    ThemesOnly = 1,
    AssetsOnly = 2
}

public interface IStoreRepository
{
    Task<int> CountActiveItemsAsync(StoreCategory? category, StoreThemeSource? themeSource, StoreCatalogScope scope, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetActiveItemsAsync(StoreCategory? category, StoreThemeSource? themeSource, StoreCatalogScope scope, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StoreItem?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StoreItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetOwnedStoreItemIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountInventoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserInventory>> GetInventoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default);
    Task AddInventoryAsync(UserInventory inventory, CancellationToken cancellationToken = default);
    Task<bool> HasInventoryAsync(Guid userId, Guid storeItemId, CancellationToken cancellationToken = default);
    Task<bool> HasPurchaseForPaymentAsync(Guid paymentTransactionId, CancellationToken cancellationToken = default);
    Task<int> CountPurchaseHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Purchase>> GetPurchaseHistoryPurchasesAsync(Guid userId, IReadOnlyList<Guid> purchaseIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentTransaction>> GetPurchaseHistorySubscriptionPaymentsAsync(Guid userId, IReadOnlyList<Guid> paymentIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid Id, DateTime CreatedAt, bool IsPurchase)>> GetPurchaseHistoryIndexAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountAllItemsAsync(StoreCategory? category, StoreThemeSource? themeSource, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetAllItemsAsync(StoreCategory? category, StoreThemeSource? themeSource, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default);
    Task UpdateStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default);

    // ── User theme submission ──────────────────────────────────────────────────
    Task<int> CountUserSubmissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetUserSubmissionsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StoreItem?> GetUserSubmissionByIdAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);

    // ── User component submission (standalone, non-Theme) ──────────────────────
    Task<int> CountUserComponentSubmissionsAsync(Guid userId, StoreCategory? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetUserComponentSubmissionsAsync(Guid userId, StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StoreItem?> GetUserComponentSubmissionByIdAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);

    // ── Inline components (attached to a mixed Theme combo) ────────────────────
    /// <summary>Returns all StoreItems that were created inline as part of the given Theme combo.</summary>
    Task<IReadOnlyList<StoreItem>> GetInlineComponentsByThemeIdAsync(Guid themeId, CancellationToken cancellationToken = default);

    /// <summary>Bulk-update status + IsActive + ReviewedAt for a set of item IDs in one shot.</summary>
    Task BulkUpdateStatusAsync(IReadOnlyList<Guid> itemIds, StoreItemStatus status, bool isActive, DateTime reviewedAt, CancellationToken cancellationToken = default);

    // ── Admin pending review ───────────────────────────────────────────────────
    Task<int> CountPendingReviewAsync(StoreCategory? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetPendingReviewAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
}
