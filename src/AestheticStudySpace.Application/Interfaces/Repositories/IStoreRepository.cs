using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IStoreRepository
{
    Task<IReadOnlyList<StoreItem>> GetActiveItemsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StoreItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default);
    Task AddInventoryAsync(UserInventory inventory, CancellationToken cancellationToken = default);
    Task<bool> HasInventoryAsync(Guid userId, Guid storeItemId, CancellationToken cancellationToken = default);
}

