using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

public record AdminStoreItemDto(
    Guid Id,
    StoreCategory Category,
    StoreThemeSource? ThemeSource,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive,
    StoreItemStatus Status,
    Guid? CreatorId,
    string? CreatorUsername,
    string? RejectionNote,
    DateTime? ReviewedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateStoreItemRequestDto(
    StoreCategory Category,
    StoreThemeSource? ThemeSource,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    List<Guid>? ThemeStickerItemIds = null,
    List<Guid>? ThemeBackgroundItemIds = null,
    List<Guid>? ThemeEffectItemIds = null,
    List<Guid>? ThemeAmbientSoundItemIds = null,
    bool IsPremium = true,
    int? CoinPrice = null,
    long? RealMoneyPriceVnd = null,
    bool IsActive = true);

public record UpdateStoreItemRequestDto(
    StoreCategory Category,
    StoreThemeSource? ThemeSource,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive,
    List<Guid>? ThemeStickerItemIds = null,
    List<Guid>? ThemeBackgroundItemIds = null,
    List<Guid>? ThemeEffectItemIds = null,
    List<Guid>? ThemeAmbientSoundItemIds = null);

public record ApproveThemeRequestDto(
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsPremium = false);

/// <summary>Admin approval DTO for standalone components (Sticker / Background / Effect / AmbientSound).</summary>
public record ApproveComponentRequestDto(
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsPremium = false);

public record RejectThemeRequestDto(string RejectionNote);

public record PatchStoreItemRequestDto(
    StoreCategory? Category = null,
    StoreThemeSource? ThemeSource = null,
    string? Name = null,
    string? Description = null,
    string? AssetUrl = null,
    string? PreviewUrl = null,
    Guid? ThemeStickerItemId = null,
    Guid? ThemeBackgroundItemId = null,
    Guid? ThemeEffectItemId = null,
    Guid? ThemeAmbientSoundItemId = null,
    List<Guid>? ThemeStickerItemIds = null,
    List<Guid>? ThemeBackgroundItemIds = null,
    List<Guid>? ThemeEffectItemIds = null,
    List<Guid>? ThemeAmbientSoundItemIds = null,
    bool? IsPremium = null,
    int? CoinPrice = null,
    long? RealMoneyPriceVnd = null,
    bool? IsActive = null);

