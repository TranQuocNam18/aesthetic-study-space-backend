using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAdminStoreService
{
    Task<PagedResult<AdminStoreItemDto>> GetItemsAsync(StoreCategory? category, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> CreateAsync(CreateStoreItemRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> UpdateAsync(Guid id, UpdateStoreItemRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
