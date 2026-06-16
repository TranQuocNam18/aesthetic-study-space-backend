using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class AdminStoreService : IAdminStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminStoreService(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdminStoreItemDto>> GetItemsAsync(
        StoreCategory? category,
        StoreThemeSource? themeSource,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountAllItemsAsync(category, themeSource, includeInactive, cancellationToken);
        var items = await _storeRepository.GetAllItemsAsync(category, themeSource, includeInactive, page, pageSize, cancellationToken);

        return new PagedResult<AdminStoreItemDto>
        {
            Items = items.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminStoreItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> CreateAsync(CreateStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        var item = new StoreItem
        {
            Category = request.Category,
            ThemeSource = request.Category == StoreCategory.Theme ? request.ThemeSource ?? StoreThemeSource.Official : null,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            PreviewUrl = request.PreviewUrl?.Trim(),
            ThemeStickerItemId = request.ThemeStickerItemId,
            ThemeBackgroundItemId = request.ThemeBackgroundItemId,
            ThemeEffectItemId = request.ThemeEffectItemId,
            ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId,
            IsPremium = request.IsPremium,
            CoinPrice = NormalizeCoinPrice(request.CoinPrice),
            RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd),
            IsActive = request.IsActive
        };

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> UpdateAsync(Guid id, UpdateStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        ValidateRequest(request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        item.Category = request.Category;
        item.ThemeSource = request.Category == StoreCategory.Theme ? request.ThemeSource ?? StoreThemeSource.Official : null;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.ThemeStickerItemId = request.ThemeStickerItemId;
        item.ThemeBackgroundItemId = request.ThemeBackgroundItemId;
        item.ThemeEffectItemId = request.ThemeEffectItemId;
        item.ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId;
        item.IsPremium = request.IsPremium;
        item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> PatchAsync(Guid id, PatchStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (request.Category is not null)
            item.Category = request.Category.Value;

        if (request.Category is not null || request.ThemeSource is not null)
        {
            item.ThemeSource = item.Category == StoreCategory.Theme ? request.ThemeSource ?? item.ThemeSource ?? StoreThemeSource.Official : null;
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");
            item.Name = request.Name.Trim();
        }

        if (request.Description is not null)
            item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.AssetUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(request.AssetUrl))
                throw new ValidationException("AssetUrl is required.");
            item.AssetUrl = request.AssetUrl.Trim();
        }

        if (request.PreviewUrl is not null)
            item.PreviewUrl = string.IsNullOrWhiteSpace(request.PreviewUrl) ? null : request.PreviewUrl.Trim();

        if (request.ThemeStickerItemId != default)
            item.ThemeStickerItemId = request.ThemeStickerItemId;

        if (request.ThemeBackgroundItemId != default)
            item.ThemeBackgroundItemId = request.ThemeBackgroundItemId;

        if (request.ThemeEffectItemId != default)
            item.ThemeEffectItemId = request.ThemeEffectItemId;

        if (request.ThemeAmbientSoundItemId != default)
            item.ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId;

        if (request.IsPremium is not null)
            item.IsPremium = request.IsPremium.Value;

        if (request.CoinPrice is not null)
            item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);

        if (request.IsActive is not null)
            item.IsActive = request.IsActive.Value;

        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        item.IsActive = false;
        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(string name, string assetUrl, int? coinPrice, long? realMoneyPriceVnd)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(assetUrl))
            throw new ValidationException("AssetUrl is required.");

        if (coinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");

        if (realMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");

    }

    private static int? NormalizeCoinPrice(int? coinPrice) =>
        coinPrice is null or <= 0 ? null : coinPrice;

    private static long? NormalizeMoneyPrice(long? price) =>
        price is null or <= 0 ? null : price;

    public async Task<PagedResult<AdminStoreItemDto>> GetPendingThemesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountPendingReviewAsync(cancellationToken);
        var items = await _storeRepository.GetPendingReviewAsync(page, pageSize, cancellationToken);

        return new PagedResult<AdminStoreItemDto>
        {
            Items = items.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminStoreItemDto> ApprovePendingThemeAsync(
        Guid id,
        ApproveThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != Domain.Enums.StoreItemStatus.PendingReview)
            throw new ValidationException("Only items with status 'PendingReview' can be approved.");

        item.Status = Domain.Enums.StoreItemStatus.Approved;
        item.IsActive = true;
        item.ThemeSource = StoreThemeSource.Community;
        item.RejectionNote = null;
        item.ReviewedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        // Admin can optionally adjust pricing before approving
        if (request.CoinPrice is not null)
            item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);

        item.IsPremium = request.IsPremium;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> RejectPendingThemeAsync(
        Guid id,
        RejectThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionNote))
            throw new ValidationException("Rejection note is required.");

        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != Domain.Enums.StoreItemStatus.PendingReview)
            throw new ValidationException("Only items with status 'PendingReview' can be rejected.");

        item.Status = Domain.Enums.StoreItemStatus.Rejected;
        item.IsActive = false;
        item.RejectionNote = request.RejectionNote.Trim();
        item.ReviewedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    private static AdminStoreItemDto ToAdminDto(StoreItem x) =>
        new(x.Id, x.Category, x.ThemeSource, x.Name, x.Description, x.AssetUrl, x.PreviewUrl,
            x.ThemeStickerItemId, x.ThemeBackgroundItemId, x.ThemeEffectItemId, x.ThemeAmbientSoundItemId,
            x.IsPremium,
            x.CoinPrice, x.RealMoneyPriceVnd, x.IsActive,
            x.Status, x.CreatorId, x.Creator?.Username,
            x.RejectionNote, x.ReviewedAt,
            x.CreatedAt, x.UpdatedAt);

    private static void ValidateThemeComponents(
        StoreCategory category,
        Guid? stickerItemId,
        Guid? backgroundItemId,
        Guid? effectItemId,
        Guid? ambientSoundItemId)
    {
        if (category != StoreCategory.Theme)
            return;

        var provided = new[] { stickerItemId, backgroundItemId, effectItemId, ambientSoundItemId }
            .Count(id => id is not null);

        if (provided < 2)
            throw new ValidationException("Theme items must include at least 2 different component types (sticker, background, effect, or ambient sound).");
    }
}
