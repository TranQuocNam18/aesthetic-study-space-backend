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

    // ── User submission review workflow ──────────────────────────────────────
    /// <summary>Returns all pending-review submissions. Pass category to filter (e.g. only Themes, only Stickers, etc.).
    /// Inline components attached to a Theme combo are excluded — they are approved/rejected alongside their parent Theme.</summary>
    Task<PagedResult<AdminStoreItemDto>> GetPendingSubmissionsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> ApprovePendingThemeAsync(Guid id, ApproveThemeRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> RejectPendingThemeAsync(Guid id, RejectThemeRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> ApprovePendingComponentAsync(Guid id, ApproveComponentRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminStoreItemDto> RejectPendingComponentAsync(Guid id, RejectThemeRequestDto request, CancellationToken cancellationToken = default);
}
