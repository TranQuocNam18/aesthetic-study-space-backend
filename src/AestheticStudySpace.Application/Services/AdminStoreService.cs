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
    private readonly IUserRepository _userRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminStoreService(
        IStoreRepository storeRepository, 
        IUserRepository userRepository,
        ICoinTransactionRepository coinTransactionRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _coinTransactionRepository = coinTransactionRepository;
        _notificationService = notificationService;
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

        var (stickerId, stickerItems) = await ProcessSlotComponentsAsync(request.ThemeStickerItemId, request.ThemeStickerItemIds, cancellationToken);
        var (backgroundId, backgroundItems) = await ProcessSlotComponentsAsync(request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds, cancellationToken);
        var (effectId, effectItems) = await ProcessSlotComponentsAsync(request.ThemeEffectItemId, request.ThemeEffectItemIds, cancellationToken);
        var (soundId, soundItems) = await ProcessSlotComponentsAsync(request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds, cancellationToken);

        var allItemsToCreate = stickerItems.Concat(backgroundItems).Concat(effectItems).Concat(soundItems).ToList();

        var item = new StoreItem
        {
            Category = request.Category,
            ThemeSource = request.Category == StoreCategory.Theme ? request.ThemeSource ?? StoreThemeSource.Official : null,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            PreviewUrl = request.PreviewUrl?.Trim(),
            ThemeStickerItemId = stickerId,
            ThemeBackgroundItemId = backgroundId,
            ThemeEffectItemId = effectId,
            ThemeAmbientSoundItemId = soundId,
            IsPremium = request.IsPremium,
            CoinPrice = NormalizeCoinPrice(request.CoinPrice),
            RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd),
            IsActive = request.IsActive
        };

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId, allItemsToCreate.Count);

        await _storeRepository.AddStoreItemAsync(item, cancellationToken);
        
        foreach (var comp in allItemsToCreate)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var comp in allItemsToCreate)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> UpdateAsync(Guid id, UpdateStoreItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        ValidateRequest(request.Name, request.AssetUrl, request.CoinPrice, request.RealMoneyPriceVnd);

        // Soft-delete old inline components before replacing
        var oldInline = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var comp in oldInline)
        {
            comp.IsDeleted = true;
            comp.DeletedAt = now;
            comp.UpdatedAt = now;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        var (stickerId, stickerItems) = await ProcessSlotComponentsAsync(request.ThemeStickerItemId, request.ThemeStickerItemIds, cancellationToken);
        var (backgroundId, backgroundItems) = await ProcessSlotComponentsAsync(request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds, cancellationToken);
        var (effectId, effectItems) = await ProcessSlotComponentsAsync(request.ThemeEffectItemId, request.ThemeEffectItemIds, cancellationToken);
        var (soundId, soundItems) = await ProcessSlotComponentsAsync(request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds, cancellationToken);

        var allItemsToCreate = stickerItems.Concat(backgroundItems).Concat(effectItems).Concat(soundItems).ToList();

        item.Category = request.Category;
        item.ThemeSource = request.Category == StoreCategory.Theme ? request.ThemeSource ?? StoreThemeSource.Official : null;
        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.ThemeStickerItemId = stickerId;
        item.ThemeBackgroundItemId = backgroundId;
        item.ThemeEffectItemId = effectId;
        item.ThemeAmbientSoundItemId = soundId;
        item.IsPremium = request.IsPremium;
        item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId, allItemsToCreate.Count);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        foreach (var comp in allItemsToCreate)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var comp in allItemsToCreate)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

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

        var patchedInlineItems = new List<StoreItem>();
        var now = DateTime.UtcNow;

        async Task HandleSlotIdsAsync(
            Guid? singularId,
            List<Guid>? multipleIds,
            StoreCategory category,
            Action<Guid?> setSlotId)
        {
            if (singularId != null || multipleIds != null)
            {
                var oldInline = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
                foreach (var oldComp in oldInline.Where(x => x.Category == category))
                {
                    oldComp.IsDeleted = true;
                    oldComp.DeletedAt = now;
                    oldComp.UpdatedAt = now;
                    await _storeRepository.UpdateStoreItemAsync(oldComp, cancellationToken);
                }

                var (firstId, items) = await ProcessSlotComponentsAsync(singularId, multipleIds, cancellationToken);
                setSlotId(firstId);
                patchedInlineItems.AddRange(items);
            }
        }

        await HandleSlotIdsAsync(request.ThemeStickerItemId, request.ThemeStickerItemIds, StoreCategory.Sticker, v => item.ThemeStickerItemId = v);
        await HandleSlotIdsAsync(request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds, StoreCategory.Background, v => item.ThemeBackgroundItemId = v);
        await HandleSlotIdsAsync(request.ThemeEffectItemId, request.ThemeEffectItemIds, StoreCategory.Effect, v => item.ThemeEffectItemId = v);
        await HandleSlotIdsAsync(request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds, StoreCategory.AmbientSound, v => item.ThemeAmbientSoundItemId = v);

        if (request.IsPremium is not null)
            item.IsPremium = request.IsPremium.Value;

        if (request.CoinPrice is not null)
            item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);

        if (request.IsActive is not null)
            item.IsActive = request.IsActive.Value;

        item.UpdatedAt = DateTime.UtcNow;

        ValidateThemeComponents(item.Category, item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId, patchedInlineItems.Count);

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        foreach (var comp in patchedInlineItems)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var comp in patchedInlineItems)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

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

    public async Task<PagedResult<AdminStoreItemDto>> GetPendingSubmissionsAsync(
        StoreCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountPendingReviewAsync(category, cancellationToken);
        var items = await _storeRepository.GetPendingReviewAsync(category, page, pageSize, cancellationToken);

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

        if (item.Category != StoreCategory.Theme)
            throw new ValidationException("Use the component approval endpoint for non-theme items.");

        var now = DateTime.UtcNow;
        item.Status = Domain.Enums.StoreItemStatus.Approved;
        item.IsActive = true;
        item.ThemeSource = StoreThemeSource.Community;
        item.RejectionNote = null;
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        if (request.CoinPrice is not null)
            item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);
        item.IsPremium = request.IsPremium;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // Cascade-approve all inline components belonging to this Theme combo
        var inlineComponents = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
        if (inlineComponents.Count > 0)
        {
            var inlineIds = inlineComponents.Select(x => x.Id).ToList();
            await _storeRepository.BulkUpdateStatusAsync(inlineIds, Domain.Enums.StoreItemStatus.Approved, isActive: true, now, cancellationToken);
        }

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

        if (item.Category != StoreCategory.Theme)
            throw new ValidationException("Use the component rejection endpoint for non-theme items.");

        var now = DateTime.UtcNow;
        item.Status = Domain.Enums.StoreItemStatus.Rejected;
        item.IsActive = false;
        item.RejectionNote = request.RejectionNote.Trim();
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // Cascade-reject all inline components belonging to this Theme combo
        var inlineComponents = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
        if (inlineComponents.Count > 0)
        {
            var inlineIds = inlineComponents.Select(x => x.Id).ToList();
            await _storeRepository.BulkUpdateStatusAsync(inlineIds, Domain.Enums.StoreItemStatus.Rejected, isActive: false, now, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> ApprovePendingComponentAsync(
        Guid id,
        ApproveComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != Domain.Enums.StoreItemStatus.PendingReview)
            throw new ValidationException("Only items with status 'PendingReview' can be approved.");

        if (item.Category == StoreCategory.Theme)
            throw new ValidationException("Use the theme approval endpoint for Theme items.");

        var now = DateTime.UtcNow;
        item.Status = Domain.Enums.StoreItemStatus.Approved;
        item.IsActive = true;
        item.RejectionNote = null;
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        if (request.CoinPrice is not null)
            item.CoinPrice = NormalizeCoinPrice(request.CoinPrice);
        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = NormalizeMoneyPrice(request.RealMoneyPriceVnd);
        item.IsPremium = request.IsPremium;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> RejectPendingComponentAsync(
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

        if (item.Category == StoreCategory.Theme)
            throw new ValidationException("Use the theme rejection endpoint for Theme items.");

        var now = DateTime.UtcNow;
        item.Status = Domain.Enums.StoreItemStatus.Rejected;
        item.IsActive = false;
        item.RejectionNote = request.RejectionNote.Trim();
        item.ReviewedAt = now;
        item.UpdatedAt = now;

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
            x.CreatedAt, x.UpdatedAt,
            x.BankAccountNumber, x.BankName, x.BankAccountOwnerName,
            x.RequestedCoinPrice, x.RequestedRealMoneyPriceVnd,
            x.IsBoughtByAdmin);

    private async Task<(Guid? firstId, List<StoreItem> itemsToCreate)> ProcessSlotComponentsAsync(
        Guid? singularId,
        List<Guid>? multipleIds,
        CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        if (singularId.HasValue && singularId != Guid.Empty)
            ids.Add(singularId.Value);
        if (multipleIds != null)
            ids.AddRange(multipleIds.Where(id => id != Guid.Empty));

        Guid? firstId = null;
        var itemsToCreate = new List<StoreItem>();

        if (ids.Count > 0)
        {
            firstId = ids[0];
            for (int i = 1; i < ids.Count; i++)
            {
                var cloned = await CloneAsInlineComponentAsync(ids[i], cancellationToken);
                itemsToCreate.Add(cloned);
            }
        }

        return (firstId, itemsToCreate);
    }

    private async Task<StoreItem> CloneAsInlineComponentAsync(Guid originalId, CancellationToken cancellationToken)
    {
        var original = await _storeRepository.GetByIdAsync(originalId, cancellationToken)
            ?? throw new NotFoundException($"Store item '{originalId}' not found.");
        return new StoreItem
        {
            Category = original.Category,
            ThemeSource = null,
            Name = original.Name,
            Description = original.Description,
            AssetUrl = original.AssetUrl,
            PreviewUrl = original.PreviewUrl,
            IsPremium = original.IsPremium,
            IsActive = true,
            CreatorId = null,
            Status = StoreItemStatus.Approved
        };
    }

    private static void ValidateThemeComponents(
        StoreCategory category,
        Guid? stickerItemId,
        Guid? backgroundItemId,
        Guid? effectItemId,
        Guid? ambientSoundItemId,
        int inlineCount = 0)
    {
        if (category != StoreCategory.Theme)
            return;

        var provided = new[] { stickerItemId, backgroundItemId, effectItemId, ambientSoundItemId }
            .Count(id => id is not null);

        if (provided + inlineCount < 2)
            throw new ValidationException("Theme items must include at least 2 different component types (sticker, background, effect, or ambient sound).");
    }

    // ── Creator Buyout Transaction & Pricing Pool Workflow ─────────────────────

    public async Task<PagedResult<AdminStoreItemDto>> GetPendingTransactionsAsync(
        StoreCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountPendingTransactionsAsync(category, cancellationToken);
        var items = await _storeRepository.GetPendingTransactionsAsync(category, page, pageSize, cancellationToken);

        return new PagedResult<AdminStoreItemDto>
        {
            Items = items.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminStoreItemDto> ApproveTransactionAsync(
        Guid id,
        AdminApproveTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != StoreItemStatus.PendingTransaction)
            throw new ValidationException("Only items with status 'PendingTransaction' can be approved.");

        var now = DateTime.UtcNow;
        item.Status = StoreItemStatus.PurchasedPendingPricing;
        item.IsBoughtByAdmin = true;
        item.IsActive = false;
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // If paying in coins and creator requested coins
        if (request.PayInCoins && item.CreatorId.HasValue)
        {
            var creator = await _userRepository.GetByIdAsync(item.CreatorId.Value, cancellationToken);
            if (creator != null && item.RequestedCoinPrice.HasValue && item.RequestedCoinPrice.Value > 0)
            {
                creator.CoinsBalance += item.RequestedCoinPrice.Value;
                await _userRepository.UpdateAsync(creator, cancellationToken);

                await _coinTransactionRepository.AddAsync(new CoinTransaction
                {
                    UserId = creator.Id,
                    Type = CoinTransactionType.Earned,
                    Amount = item.RequestedCoinPrice.Value,
                    Reason = $"BuyoutPayout:{item.Name}"
                }, cancellationToken);
            }
        }

        // Cascade-update all inline components if theme
        if (item.Category == StoreCategory.Theme)
        {
            var inlineComponents = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
            if (inlineComponents.Count > 0)
            {
                var inlineIds = inlineComponents.Select(x => x.Id).ToList();
                await _storeRepository.BulkUpdateStatusAsync(inlineIds, StoreItemStatus.PurchasedPendingPricing, isActive: false, now, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create Web Notification for the creator
        if (item.CreatorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                userId: item.CreatorId.Value,
                isForAdmin: false,
                title: "Transaction Approved",
                message: $"Your submission request for '{item.Name}' has been approved by Admin.",
                cancellationToken);
        }

        return ToAdminDto(item);
    }

    public async Task<AdminStoreItemDto> RejectTransactionAsync(
        Guid id,
        RejectThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionNote))
            throw new ValidationException("Rejection note is required.");

        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != StoreItemStatus.PendingTransaction)
            throw new ValidationException("Only items with status 'PendingTransaction' can be rejected.");

        var now = DateTime.UtcNow;
        item.Status = StoreItemStatus.Rejected;
        item.IsActive = false;
        item.RejectionNote = request.RejectionNote.Trim();
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // Cascade-reject all inline components if theme
        if (item.Category == StoreCategory.Theme)
        {
            var inlineComponents = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
            if (inlineComponents.Count > 0)
            {
                var inlineIds = inlineComponents.Select(x => x.Id).ToList();
                await _storeRepository.BulkUpdateStatusAsync(inlineIds, StoreItemStatus.Rejected, isActive: false, now, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create Web Notification for the creator
        if (item.CreatorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                userId: item.CreatorId.Value,
                isForAdmin: false,
                title: "Transaction Rejected",
                message: $"Your submission request for '{item.Name}' has been rejected. Reason: {request.RejectionNote}",
                cancellationToken);
        }

        return ToAdminDto(item);
    }

    public async Task<PagedResult<AdminStoreItemDto>> GetPurchasedPendingPricingAsync(
        StoreCategory? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _storeRepository.CountPurchasedPendingPricingAsync(category, cancellationToken);
        var items = await _storeRepository.GetPurchasedPendingPricingAsync(category, page, pageSize, cancellationToken);

        return new PagedResult<AdminStoreItemDto>
        {
            Items = items.Select(ToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AdminStoreItemDto> PriceAndPublishAsync(
        Guid id,
        AdminPriceAndPublishDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.CoinPrice <= 0)
            throw new ValidationException("CoinPrice must be greater than zero.");

        var item = await _storeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Store item not found.");

        if (item.Status != StoreItemStatus.PurchasedPendingPricing)
            throw new ValidationException("Only items in 'PurchasedPendingPricing' status can be published.");

        var now = DateTime.UtcNow;
        item.CoinPrice = request.CoinPrice;
        item.IsPremium = request.IsPremium;
        item.Status = StoreItemStatus.Approved;
        item.IsActive = true;
        item.ThemeSource = StoreThemeSource.Community;
        item.ReviewedAt = now;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // Cascade-approve all inline components if theme
        if (item.Category == StoreCategory.Theme)
        {
            var inlineComponents = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
            if (inlineComponents.Count > 0)
            {
                var inlineIds = inlineComponents.Select(x => x.Id).ToList();
                await _storeRepository.BulkUpdateStatusAsync(inlineIds, StoreItemStatus.Approved, isActive: true, now, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create Web Notification for the creator
        if (item.CreatorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                userId: item.CreatorId.Value,
                isForAdmin: false,
                title: "Asset Published",
                message: $"Your design '{item.Name}' has been priced and published to the store.",
                cancellationToken);
        }

        return ToAdminDto(item);
    }
}
