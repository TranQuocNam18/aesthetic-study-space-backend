using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class UserThemeService : IUserThemeService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserThemeService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserThemeSubmissionDto> SubmitThemeAsync(
        Guid userId,
        SubmitThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate base fields
        ValidateThemeRequest(request);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.IsBanned)
            throw new UnauthorizedException("Banned users cannot submit themes.");

        // Build inline component StoreItems (Luồng C)
        var (inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound)
            = BuildInlineComponents(userId, request);

        // Determine the effective slot IDs (from store or inline)
        var stickerItemId   = ResolveSlot(request.ThemeStickerItemId,     inlineSticker);
        var backgroundItemId = ResolveSlot(request.ThemeBackgroundItemId, inlineBackground);
        var effectItemId    = ResolveSlot(request.ThemeEffectItemId,      inlineEffect);
        var soundItemId     = ResolveSlot(request.ThemeAmbientSoundItemId, inlineAmbientSound);

        var inlineItemsCount = new[] { inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound }
            .Count(x => x is not null);

        ValidateThemeComponentCount(stickerItemId, backgroundItemId, effectItemId, soundItemId, inlineItemsCount);

        // Create the Theme StoreItem
        var theme = new StoreItem
        {
            Category = StoreCategory.Theme,
            ThemeSource = StoreThemeSource.Community,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            PreviewUrl = request.PreviewUrl?.Trim(),
            ThemeStickerItemId = stickerItemId,
            ThemeBackgroundItemId = backgroundItemId,
            ThemeEffectItemId = effectItemId,
            ThemeAmbientSoundItemId = soundItemId,
            IsPremium = false,
            CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null,
            RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null,
            IsActive = false,
            CreatorId = userId,
            Status = StoreItemStatus.PendingReview
        };

        await _storeRepository.AddStoreItemAsync(theme, cancellationToken);

        // Persist inline components first so they get IDs before we link them
        var inlineItems = new[] { inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound }
            .Where(x => x is not null)
            .Cast<StoreItem>()
            .ToList();

        foreach (var comp in inlineItems)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        // SaveChanges once to generate IDs for all items
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Now update the inline components' ParentThemeId to point to the Theme we just created
        foreach (var comp in inlineItems)
        {
            comp.ParentThemeId = theme.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        // Also update Theme's ThemeXxxItemId slots that came from inline items
        // (they were set to Guid.Empty placeholder; now we have the real IDs)
        theme.ThemeStickerItemId     = inlineSticker     is not null ? inlineSticker.Id     : theme.ThemeStickerItemId;
        theme.ThemeBackgroundItemId  = inlineBackground  is not null ? inlineBackground.Id  : theme.ThemeBackgroundItemId;
        theme.ThemeEffectItemId      = inlineEffect      is not null ? inlineEffect.Id      : theme.ThemeEffectItemId;
        theme.ThemeAmbientSoundItemId = inlineAmbientSound is not null ? inlineAmbientSound.Id : theme.ThemeAmbientSoundItemId;

        await _storeRepository.UpdateStoreItemAsync(theme, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(theme, inlineItems);
    }

    public async Task<PagedResult<UserThemeSubmissionDto>> GetMySubmissionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _storeRepository.CountUserSubmissionsAsync(userId, cancellationToken);
        var items = await _storeRepository.GetUserSubmissionsAsync(userId, page, pageSize, cancellationToken);

        // Load inline components for each theme in one batch
        var themeIds = items.Select(x => x.Id).ToList();
        var allInline = await LoadInlineComponentsForThemesAsync(themeIds, cancellationToken);

        return new PagedResult<UserThemeSubmissionDto>
        {
            Items = items.Select(x => ToDto(x, allInline.GetValueOrDefault(x.Id, []))).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<UserThemeSubmissionDto> GetMySubmissionByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        var inline = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
        return ToDto(item, inline);
    }

    public async Task WithdrawSubmissionAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot withdraw an approved theme that is live in the store.");

        var now = DateTime.UtcNow;
        item.IsDeleted = true;
        item.DeletedAt = now;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        // Soft-delete all inline components as well
        var inline = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
        foreach (var comp in inline)
        {
            comp.IsDeleted = true;
            comp.DeletedAt = now;
            comp.UpdatedAt = now;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserThemeSubmissionDto> UpdateThemeAsync(
        Guid userId,
        Guid id,
        SubmitThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateThemeRequest(request);

        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved theme that is live in the store.");

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

        var (inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound)
            = BuildInlineComponents(userId, request);

        var stickerItemId    = ResolveSlot(request.ThemeStickerItemId,      inlineSticker);
        var backgroundItemId = ResolveSlot(request.ThemeBackgroundItemId,   inlineBackground);
        var effectItemId     = ResolveSlot(request.ThemeEffectItemId,       inlineEffect);
        var soundItemId      = ResolveSlot(request.ThemeAmbientSoundItemId, inlineAmbientSound);

        var inlineItemsCount = new[] { inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound }
            .Count(x => x is not null);

        ValidateThemeComponentCount(stickerItemId, backgroundItemId, effectItemId, soundItemId, inlineItemsCount);

        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.ThemeStickerItemId = stickerItemId;
        item.ThemeBackgroundItemId = backgroundItemId;
        item.ThemeEffectItemId = effectItemId;
        item.ThemeAmbientSoundItemId = soundItemId;
        item.CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null;
        item.RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null;
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        var newInlineItems = new[] { inlineSticker, inlineBackground, inlineEffect, inlineAmbientSound }
            .Where(x => x is not null).Cast<StoreItem>().ToList();

        foreach (var comp in newInlineItems)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update ParentThemeId and resolve IDs
        foreach (var comp in newInlineItems)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        item.ThemeStickerItemId      = inlineSticker     is not null ? inlineSticker.Id      : item.ThemeStickerItemId;
        item.ThemeBackgroundItemId   = inlineBackground  is not null ? inlineBackground.Id   : item.ThemeBackgroundItemId;
        item.ThemeEffectItemId       = inlineEffect      is not null ? inlineEffect.Id       : item.ThemeEffectItemId;
        item.ThemeAmbientSoundItemId = inlineAmbientSound is not null ? inlineAmbientSound.Id : item.ThemeAmbientSoundItemId;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item, newInlineItems);
    }

    public async Task<UserThemeSubmissionDto> PatchThemeAsync(
        Guid userId,
        Guid id,
        PatchThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _storeRepository.GetUserSubmissionByIdAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Theme submission not found.");

        if (item.Status == StoreItemStatus.Approved)
            throw new ValidationException("Cannot update an approved theme that is live in the store.");

        var now = DateTime.UtcNow;

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name cannot be empty.");
            item.Name = request.Name.Trim();
        }

        if (request.Description is not null)
            item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.AssetUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(request.AssetUrl))
                throw new ValidationException("AssetUrl cannot be empty.");
            item.AssetUrl = request.AssetUrl.Trim();
        }

        if (request.PreviewUrl is not null)
            item.PreviewUrl = string.IsNullOrWhiteSpace(request.PreviewUrl) ? null : request.PreviewUrl.Trim();

        if (request.ThemeStickerItemId != default)
            item.ThemeStickerItemId = request.ThemeStickerItemId == Guid.Empty ? null : request.ThemeStickerItemId;

        if (request.ThemeBackgroundItemId != default)
            item.ThemeBackgroundItemId = request.ThemeBackgroundItemId == Guid.Empty ? null : request.ThemeBackgroundItemId;

        if (request.ThemeEffectItemId != default)
            item.ThemeEffectItemId = request.ThemeEffectItemId == Guid.Empty ? null : request.ThemeEffectItemId;

        if (request.ThemeAmbientSoundItemId != default)
            item.ThemeAmbientSoundItemId = request.ThemeAmbientSoundItemId == Guid.Empty ? null : request.ThemeAmbientSoundItemId;

        if (request.CoinPrice is not null)
            item.CoinPrice = request.CoinPrice > 0 ? request.CoinPrice : null;

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = request.RealMoneyPriceVnd > 0 ? request.RealMoneyPriceVnd : null;

        // Handle new inline components in PATCH (add/replace per slot)
        var patchedInlineItems = new List<StoreItem>();

        async Task HandleInlineSlot(
            InlineComponentDto? inlineDto,
            StoreCategory category,
            Func<Guid?> getOldInlineId,
            Action<Guid?> setSlotId)
        {
            if (inlineDto is null) return;
            ValidateInlineComponent(inlineDto, category);

            // Remove old inline for this slot if it exists
            var oldId = getOldInlineId();
            if (oldId.HasValue)
            {
                var oldComp = await _storeRepository.GetUserComponentSubmissionByIdAsync(userId, oldId.Value, cancellationToken);
                if (oldComp is not null && oldComp.ParentThemeId == item.Id)
                {
                    oldComp.IsDeleted = true;
                    oldComp.DeletedAt = now;
                    oldComp.UpdatedAt = now;
                    await _storeRepository.UpdateStoreItemAsync(oldComp, cancellationToken);
                }
            }

            var newComp = CreateInlineItem(userId, inlineDto);
            await _storeRepository.AddStoreItemAsync(newComp, cancellationToken);
            patchedInlineItems.Add(newComp);
            setSlotId(null); // Will be resolved after SaveChanges
        }

        await HandleInlineSlot(request.InlineSticker,     StoreCategory.Sticker,     () => item.ThemeStickerItemId,      v => item.ThemeStickerItemId = v);
        await HandleInlineSlot(request.InlineBackground,  StoreCategory.Background,  () => item.ThemeBackgroundItemId,   v => item.ThemeBackgroundItemId = v);
        await HandleInlineSlot(request.InlineEffect,      StoreCategory.Effect,      () => item.ThemeEffectItemId,       v => item.ThemeEffectItemId = v);
        await HandleInlineSlot(request.InlineAmbientSound, StoreCategory.AmbientSound, () => item.ThemeAmbientSoundItemId, v => item.ThemeAmbientSoundItemId = v);

        ValidateThemeComponentCount(item.ThemeStickerItemId, item.ThemeBackgroundItemId, item.ThemeEffectItemId, item.ThemeAmbientSoundItemId,
            patchedInlineItems.Count);

        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Resolve inline IDs post-save
        foreach (var comp in patchedInlineItems)
        {
            comp.ParentThemeId = item.Id;
            if (comp.Category == StoreCategory.Sticker && request.InlineSticker is not null)
                item.ThemeStickerItemId = comp.Id;
            else if (comp.Category == StoreCategory.Background && request.InlineBackground is not null)
                item.ThemeBackgroundItemId = comp.Id;
            else if (comp.Category == StoreCategory.Effect && request.InlineEffect is not null)
                item.ThemeEffectItemId = comp.Id;
            else if (comp.Category == StoreCategory.AmbientSound && request.InlineAmbientSound is not null)
                item.ThemeAmbientSoundItemId = comp.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        if (patchedInlineItems.Count > 0)
        {
            await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var allInline = await _storeRepository.GetInlineComponentsByThemeIdAsync(item.Id, cancellationToken);
        return ToDto(item, allInline);
    }

    // ── Private Helpers ──────────────────────────────────────────────────────────

    private static void ValidateThemeRequest(SubmitThemeRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.AssetUrl))
            throw new ValidationException("AssetUrl is required.");
        if (request.CoinPrice is < 0)
            throw new ValidationException("CoinPrice cannot be negative.");
        if (request.RealMoneyPriceVnd is < 0)
            throw new ValidationException("RealMoneyPriceVnd cannot be negative.");

        // Validate no slot has both an existing ID and an inline component
        ValidateNoSlotConflict(request.ThemeStickerItemId,      request.InlineSticker,      "Sticker");
        ValidateNoSlotConflict(request.ThemeBackgroundItemId,   request.InlineBackground,   "Background");
        ValidateNoSlotConflict(request.ThemeEffectItemId,       request.InlineEffect,       "Effect");
        ValidateNoSlotConflict(request.ThemeAmbientSoundItemId, request.InlineAmbientSound, "AmbientSound");

        // Validate inline component fields
        if (request.InlineSticker is not null)      ValidateInlineComponent(request.InlineSticker,      StoreCategory.Sticker);
        if (request.InlineBackground is not null)   ValidateInlineComponent(request.InlineBackground,   StoreCategory.Background);
        if (request.InlineEffect is not null)       ValidateInlineComponent(request.InlineEffect,       StoreCategory.Effect);
        if (request.InlineAmbientSound is not null) ValidateInlineComponent(request.InlineAmbientSound, StoreCategory.AmbientSound);
    }

    private static void ValidateNoSlotConflict(Guid? existingId, InlineComponentDto? inline, string slotName)
    {
        if (existingId.HasValue && existingId != Guid.Empty && inline is not null)
            throw new ValidationException(
                $"Cannot specify both a store item ID and an inline component for the {slotName} slot.");
    }

    private static void ValidateInlineComponent(InlineComponentDto dto, StoreCategory expectedCategory)
    {
        if (dto.Category != expectedCategory)
            throw new ValidationException($"Inline component category must be {expectedCategory}.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException($"Inline {expectedCategory} name is required.");
        if (string.IsNullOrWhiteSpace(dto.AssetUrl))
            throw new ValidationException($"Inline {expectedCategory} AssetUrl is required.");
    }

    private static (StoreItem? sticker, StoreItem? background, StoreItem? effect, StoreItem? sound)
        BuildInlineComponents(Guid userId, SubmitThemeRequestDto request)
    {
        return (
            request.InlineSticker     is not null ? CreateInlineItem(userId, request.InlineSticker)     : null,
            request.InlineBackground  is not null ? CreateInlineItem(userId, request.InlineBackground)  : null,
            request.InlineEffect      is not null ? CreateInlineItem(userId, request.InlineEffect)      : null,
            request.InlineAmbientSound is not null ? CreateInlineItem(userId, request.InlineAmbientSound) : null
        );
    }

    private static StoreItem CreateInlineItem(Guid userId, InlineComponentDto dto) =>
        new()
        {
            Category = dto.Category,
            ThemeSource = null,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            AssetUrl = dto.AssetUrl.Trim(),
            PreviewUrl = dto.PreviewUrl?.Trim(),
            IsPremium = false,
            IsActive = false,
            CreatorId = userId,
            Status = StoreItemStatus.PendingReview
            // ParentThemeId will be set after the Theme is saved and gets an Id
        };

    /// <summary>
    /// Returns the ID to use for a theme slot.
    /// If an inline component is provided, use a placeholder Guid that will be replaced after save.
    /// If an existing store item ID is provided, use that directly.
    /// </summary>
    private static Guid? ResolveSlot(Guid? existingId, StoreItem? inlineItem)
    {
        if (inlineItem is not null) return null; // will be resolved after SaveChanges
        if (existingId == Guid.Empty) return null;
        return existingId;
    }

    private static void ValidateThemeComponentCount(
        Guid? stickerItemId,
        Guid? backgroundItemId,
        Guid? effectItemId,
        Guid? soundItemId,
        int pendingInlineCount = 0)
    {
        var provided = new[] { stickerItemId, backgroundItemId, effectItemId, soundItemId }
            .Count(id => id is not null);

        if (provided + pendingInlineCount < 2)
            throw new ValidationException(
                "Theme submissions must include at least 2 different component types " +
                "(sticker, background, effect, or ambient sound).");
    }

    private async Task<Dictionary<Guid, IReadOnlyList<StoreItem>>> LoadInlineComponentsForThemesAsync(
        List<Guid> themeIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, IReadOnlyList<StoreItem>>();
        foreach (var tid in themeIds)
        {
            var inline = await _storeRepository.GetInlineComponentsByThemeIdAsync(tid, cancellationToken);
            result[tid] = inline;
        }
        return result;
    }

    private static UserThemeSubmissionDto ToDto(StoreItem x, IReadOnlyList<StoreItem> inlineComponents) =>
        new(x.Id, x.Name, x.Description, x.AssetUrl, x.PreviewUrl,
            x.ThemeStickerItemId, x.ThemeBackgroundItemId, x.ThemeEffectItemId, x.ThemeAmbientSoundItemId,
            x.CoinPrice, x.RealMoneyPriceVnd,
            x.ThemeSource ?? StoreThemeSource.Community,
            x.Status, x.RejectionNote,
            x.CreatedAt, x.ReviewedAt,
            inlineComponents.Select(UserComponentService.ToDto).ToList());
}
