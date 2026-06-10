using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

public record StoreItemDto(
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
    bool? IsOwned = null,
    bool CanBuyWithCoins = false,
    bool CanBuyWithMoney = false);

public record UserInventoryItemDto(
    Guid InventoryId,
    Guid StoreItemId,
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
    DateTime AcquiredAt);

public record StorePurchaseResultDto(
    bool Purchased,
    Guid StoreItemId,
    int RemainingCoins);

public record BuyWithCoinsRequestDto(Guid StoreItemId);

public enum PurchaseHistoryKind
{
    StoreItem = 0,
    CoinPack = 1,
    Subscription = 2
}

public record PurchaseHistoryItemDto(
    Guid Id,
    PurchaseHistoryKind Kind,
    string Title,
    string? Description,
    int? CoinsSpent,
    long? AmountVnd,
    string Currency,
    PaymentProvider? PaymentProvider,
    string? TransactionCode,
    Guid? StoreItemId,
    StoreCategory? StoreCategory,
    StoreThemeSource? ThemeSource,
    string? StoreItemAssetUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    DateTime PurchasedAt);

