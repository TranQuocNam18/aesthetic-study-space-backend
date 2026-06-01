using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IStoreService
{
    Task<IReadOnlyList<StoreItemDto>> GetItemsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<object> BuyWithCoinsAsync(Guid userId, BuyWithCoinsRequestDto request, CancellationToken cancellationToken = default);
}

