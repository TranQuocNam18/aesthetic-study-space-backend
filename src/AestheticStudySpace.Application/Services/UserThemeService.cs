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
    private readonly IMissionService _missionService;
    private readonly IUnitOfWork _unitOfWork;

    public UserThemeService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IMissionService missionService,
        IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _missionService = missionService;
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

        // Gather components for each slot
        var (stickerId, stickerItems) = await ProcessSlotComponentsAsync(
            request.ThemeStickerItemId, request.ThemeStickerItemIds,
            request.InlineSticker, request.InlineStickers,
            StoreCategory.Sticker, userId, cancellationToken);

        var (backgroundId, backgroundItems) = await ProcessSlotComponentsAsync(
            request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds,
            request.InlineBackground, request.InlineBackgrounds,
            StoreCategory.Background, userId, cancellationToken);

        var (effectId, effectItems) = await ProcessSlotComponentsAsync(
            request.ThemeEffectItemId, request.ThemeEffectItemIds,
            request.InlineEffect, request.InlineEffects,
            StoreCategory.Effect, userId, cancellationToken);

        var (soundId, soundItems) = await ProcessSlotComponentsAsync(
            request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds,
            request.InlineAmbientSound, request.InlineAmbientSounds,
            StoreCategory.AmbientSound, userId, cancellationToken);

        var allItemsToCreate = stickerItems.Concat(backgroundItems).Concat(effectItems).Concat(soundItems).ToList();

        var providedCount = (stickerId != null || stickerItems.Any() ? 1 : 0)
            + (backgroundId != null || backgroundItems.Any() ? 1 : 0)
            + (effectId != null || effectItems.Any() ? 1 : 0)
            + (soundId != null || soundItems.Any() ? 1 : 0);

        if (providedCount < 2)
            throw new ValidationException("Theme submissions must include at least 2 different component types (sticker, background, effect, or ambient sound).");

        // Create the Theme StoreItem
        var theme = new StoreItem
        {
            Category = StoreCategory.Theme,
            ThemeSource = StoreThemeSource.Community,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssetUrl = request.AssetUrl.Trim(),
            PreviewUrl = request.PreviewUrl?.Trim(),
            ThemeStickerItemId = stickerId,
            ThemeBackgroundItemId = backgroundId,
            ThemeEffectItemId = effectId,
            ThemeAmbientSoundItemId = soundId,
            IsPremium = false,
            CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null,
            RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null,
            IsActive = false,
            CreatorId = userId,
            Status = StoreItemStatus.PendingReview
        };

        await _storeRepository.AddStoreItemAsync(theme, cancellationToken);

        foreach (var comp in allItemsToCreate)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        // SaveChanges once to generate IDs for all items
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Now update the inline components' ParentThemeId to point to the Theme we just created
        foreach (var comp in allItemsToCreate)
        {
            comp.ParentThemeId = theme.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        // Also update Theme's ThemeXxxItemId slots that came from inline items
        if (theme.ThemeStickerItemId == null && stickerItems.Any())
            theme.ThemeStickerItemId = stickerItems.First().Id;
        if (theme.ThemeBackgroundItemId == null && backgroundItems.Any())
            theme.ThemeBackgroundItemId = backgroundItems.First().Id;
        if (theme.ThemeEffectItemId == null && effectItems.Any())
            theme.ThemeEffectItemId = effectItems.First().Id;
        if (theme.ThemeAmbientSoundItemId == null && soundItems.Any())
            theme.ThemeAmbientSoundItemId = soundItems.First().Id;

        await _storeRepository.UpdateStoreItemAsync(theme, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Trigger mission: Share layout
        await _missionService.IncrementByTriggerKeyAsync(userId, "share_layout", 1, cancellationToken);

        return ToDto(theme, allItemsToCreate);
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

        // Gather components for each slot
        var (stickerId, stickerItems) = await ProcessSlotComponentsAsync(
            request.ThemeStickerItemId, request.ThemeStickerItemIds,
            request.InlineSticker, request.InlineStickers,
            StoreCategory.Sticker, userId, cancellationToken);

        var (backgroundId, backgroundItems) = await ProcessSlotComponentsAsync(
            request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds,
            request.InlineBackground, request.InlineBackgrounds,
            StoreCategory.Background, userId, cancellationToken);

        var (effectId, effectItems) = await ProcessSlotComponentsAsync(
            request.ThemeEffectItemId, request.ThemeEffectItemIds,
            request.InlineEffect, request.InlineEffects,
            StoreCategory.Effect, userId, cancellationToken);

        var (soundId, soundItems) = await ProcessSlotComponentsAsync(
            request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds,
            request.InlineAmbientSound, request.InlineAmbientSounds,
            StoreCategory.AmbientSound, userId, cancellationToken);

        var allItemsToCreate = stickerItems.Concat(backgroundItems).Concat(effectItems).Concat(soundItems).ToList();

        var providedCount = (stickerId != null || stickerItems.Any() ? 1 : 0)
            + (backgroundId != null || backgroundItems.Any() ? 1 : 0)
            + (effectId != null || effectItems.Any() ? 1 : 0)
            + (soundId != null || soundItems.Any() ? 1 : 0);

        if (providedCount < 2)
            throw new ValidationException("Theme submissions must include at least 2 different component types (sticker, background, effect, or ambient sound).");

        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.AssetUrl = request.AssetUrl.Trim();
        item.PreviewUrl = request.PreviewUrl?.Trim();
        item.ThemeStickerItemId = stickerId;
        item.ThemeBackgroundItemId = backgroundId;
        item.ThemeEffectItemId = effectId;
        item.ThemeAmbientSoundItemId = soundId;
        item.CoinPrice = request.CoinPrice is > 0 ? request.CoinPrice : null;
        item.RealMoneyPriceVnd = request.RealMoneyPriceVnd is > 0 ? request.RealMoneyPriceVnd : null;
        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);

        foreach (var comp in allItemsToCreate)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update ParentThemeId and resolve IDs
        foreach (var comp in allItemsToCreate)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        if (item.ThemeStickerItemId == null && stickerItems.Any())
            item.ThemeStickerItemId = stickerItems.First().Id;
        if (item.ThemeBackgroundItemId == null && backgroundItems.Any())
            item.ThemeBackgroundItemId = backgroundItems.First().Id;
        if (item.ThemeEffectItemId == null && effectItems.Any())
            item.ThemeEffectItemId = effectItems.First().Id;
        if (item.ThemeAmbientSoundItemId == null && soundItems.Any())
            item.ThemeAmbientSoundItemId = soundItems.First().Id;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item, allItemsToCreate);
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

        if (request.CoinPrice is not null)
            item.CoinPrice = request.CoinPrice > 0 ? request.CoinPrice : null;

        if (request.RealMoneyPriceVnd is not null)
            item.RealMoneyPriceVnd = request.RealMoneyPriceVnd > 0 ? request.RealMoneyPriceVnd : null;

        var patchedInlineItems = new List<StoreItem>();

        async Task HandleInlineSlot(
            InlineComponentDto? inlineDto,
            List<InlineComponentDto>? inlineDtos,
            Guid? existingId,
            List<Guid>? existingIds,
            StoreCategory category,
            Func<Guid?> getOldInlineId,
            Action<Guid?> setSlotId)
        {
            // If any modifications are requested for this slot
            if (inlineDto != null || inlineDtos != null || existingId != null || existingIds != null)
            {
                // Remove old inline components for this slot if any exist
                var oldInline = await _storeRepository.GetInlineComponentsByThemeIdAsync(id, cancellationToken);
                foreach (var oldComp in oldInline.Where(x => x.Category == category))
                {
                    oldComp.IsDeleted = true;
                    oldComp.DeletedAt = now;
                    oldComp.UpdatedAt = now;
                    await _storeRepository.UpdateStoreItemAsync(oldComp, cancellationToken);
                }

                var (firstId, items) = await ProcessSlotComponentsAsync(existingId, existingIds, inlineDto, inlineDtos, category, userId, cancellationToken);
                setSlotId(firstId);
                patchedInlineItems.AddRange(items);
            }
        }

        await HandleInlineSlot(request.InlineSticker, request.InlineStickers, request.ThemeStickerItemId, request.ThemeStickerItemIds, StoreCategory.Sticker, () => item.ThemeStickerItemId, v => item.ThemeStickerItemId = v);
        await HandleInlineSlot(request.InlineBackground, request.InlineBackgrounds, request.ThemeBackgroundItemId, request.ThemeBackgroundItemIds, StoreCategory.Background, () => item.ThemeBackgroundItemId, v => item.ThemeBackgroundItemId = v);
        await HandleInlineSlot(request.InlineEffect, request.InlineEffects, request.ThemeEffectItemId, request.ThemeEffectItemIds, StoreCategory.Effect, () => item.ThemeEffectItemId, v => item.ThemeEffectItemId = v);
        await HandleInlineSlot(request.InlineAmbientSound, request.InlineAmbientSounds, request.ThemeAmbientSoundItemId, request.ThemeAmbientSoundItemIds, StoreCategory.AmbientSound, () => item.ThemeAmbientSoundItemId, v => item.ThemeAmbientSoundItemId = v);

        item.Status = StoreItemStatus.PendingReview;
        item.RejectionNote = null;
        item.UpdatedAt = now;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        
        foreach (var comp in patchedInlineItems)
            await _storeRepository.AddStoreItemAsync(comp, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Resolve inline IDs post-save
        foreach (var comp in patchedInlineItems)
        {
            comp.ParentThemeId = item.Id;
            await _storeRepository.UpdateStoreItemAsync(comp, cancellationToken);
        }

        if (item.ThemeStickerItemId == null && patchedInlineItems.Any(x => x.Category == StoreCategory.Sticker))
            item.ThemeStickerItemId = patchedInlineItems.First(x => x.Category == StoreCategory.Sticker).Id;
        if (item.ThemeBackgroundItemId == null && patchedInlineItems.Any(x => x.Category == StoreCategory.Background))
            item.ThemeBackgroundItemId = patchedInlineItems.First(x => x.Category == StoreCategory.Background).Id;
        if (item.ThemeEffectItemId == null && patchedInlineItems.Any(x => x.Category == StoreCategory.Effect))
            item.ThemeEffectItemId = patchedInlineItems.First(x => x.Category == StoreCategory.Effect).Id;
        if (item.ThemeAmbientSoundItemId == null && patchedInlineItems.Any(x => x.Category == StoreCategory.AmbientSound))
            item.ThemeAmbientSoundItemId = patchedInlineItems.First(x => x.Category == StoreCategory.AmbientSound).Id;

        await _storeRepository.UpdateStoreItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        // Validate inline component fields
        if (request.InlineSticker is not null)      ValidateInlineComponent(request.InlineSticker,      StoreCategory.Sticker);
        if (request.InlineBackground is not null)   ValidateInlineComponent(request.InlineBackground,   StoreCategory.Background);
        if (request.InlineEffect is not null)       ValidateInlineComponent(request.InlineEffect,       StoreCategory.Effect);
        if (request.InlineAmbientSound is not null) ValidateInlineComponent(request.InlineAmbientSound, StoreCategory.AmbientSound);

        if (request.InlineStickers != null)
        {
            foreach (var s in request.InlineStickers)
                ValidateInlineComponent(s, StoreCategory.Sticker);
        }
        if (request.InlineBackgrounds != null)
        {
            foreach (var b in request.InlineBackgrounds)
                ValidateInlineComponent(b, StoreCategory.Background);
        }
        if (request.InlineEffects != null)
        {
            foreach (var e in request.InlineEffects)
                ValidateInlineComponent(e, StoreCategory.Effect);
        }
        if (request.InlineAmbientSounds != null)
        {
            foreach (var a in request.InlineAmbientSounds)
                ValidateInlineComponent(a, StoreCategory.AmbientSound);
        }
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

    private async Task<(Guid? firstId, List<StoreItem> itemsToCreate)> ProcessSlotComponentsAsync(
        Guid? singularId,
        List<Guid>? multipleIds,
        InlineComponentDto? singularInline,
        List<InlineComponentDto>? multipleInlines,
        StoreCategory category,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        if (singularId.HasValue && singularId != Guid.Empty)
            ids.Add(singularId.Value);
        if (multipleIds != null)
            ids.AddRange(multipleIds.Where(id => id != Guid.Empty));

        var inlines = new List<InlineComponentDto>();
        if (singularInline != null)
            inlines.Add(singularInline);
        if (multipleInlines != null)
            inlines.AddRange(multipleInlines);

        Guid? firstId = null;
        var itemsToCreate = new List<StoreItem>();

        if (ids.Count > 0)
        {
            firstId = ids[0];
            for (int i = 1; i < ids.Count; i++)
            {
                var cloned = await CloneAsInlineComponentAsync(ids[i], userId, cancellationToken);
                itemsToCreate.Add(cloned);
            }

            foreach (var inline in inlines)
            {
                itemsToCreate.Add(CreateInlineItem(userId, inline));
            }
        }
        else if (inlines.Count > 0)
        {
            var firstInlineItem = CreateInlineItem(userId, inlines[0]);
            itemsToCreate.Add(firstInlineItem);
            
            for (int i = 1; i < inlines.Count; i++)
            {
                itemsToCreate.Add(CreateInlineItem(userId, inlines[i]));
            }
        }

        return (firstId, itemsToCreate);
    }

    private async Task<StoreItem> CloneAsInlineComponentAsync(Guid originalId, Guid? creatorId, CancellationToken cancellationToken)
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
            IsActive = creatorId == null || creatorId == Guid.Empty,
            CreatorId = creatorId,
            Status = creatorId == null || creatorId == Guid.Empty ? StoreItemStatus.AdminCreated : StoreItemStatus.PendingReview
        };
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
        };

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
