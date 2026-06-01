using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

public record StoreItemDto(
    Guid Id,
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive);

public record BuyWithCoinsRequestDto(Guid StoreItemId);

