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
    Task<int> CountActiveItemsAsync(StoreCategory? category, StoreCatalogScope scope, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetActiveItemsAsync(StoreCategory? category, StoreCatalogScope scope, int page, int pageSize, CancellationToken cancellationToken = default);
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
    Task<int> CountAllItemsAsync(StoreCategory? category, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoreItem>> GetAllItemsAsync(StoreCategory? category, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default);
    Task UpdateStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default);
}

