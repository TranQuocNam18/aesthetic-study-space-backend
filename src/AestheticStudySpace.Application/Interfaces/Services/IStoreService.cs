using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IStoreService
{
    Task<PagedResult<StoreItemDto>> GetCatalogAsync(
        StoreCategory? category,
        StoreCatalogScope scope,
        Guid? viewerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<StoreItemDto> GetItemAsync(Guid itemId, Guid? viewerUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<UserInventoryItemDto>> GetInventoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<StorePurchaseResultDto> BuyWithCoinsAsync(Guid userId, BuyWithCoinsRequestDto request, CancellationToken cancellationToken = default);

    Task<PagedResult<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}

