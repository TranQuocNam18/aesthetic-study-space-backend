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

    // ── User theme review workflow ─────────────────────────────────────────────
    Task<PagedResult<AdminStoreItemDto>> GetPendingThemesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> ApprovePendingThemeAsync(Guid id, ApproveThemeRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> RejectPendingThemeAsync(Guid id, RejectThemeRequestDto request, CancellationToken cancellationToken = default);
}
