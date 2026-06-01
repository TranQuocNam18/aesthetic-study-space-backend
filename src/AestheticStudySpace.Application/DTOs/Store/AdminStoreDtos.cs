using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

public record AdminStoreItemDto(
    Guid Id,
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateStoreItemRequestDto(
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive = true);

public record UpdateStoreItemRequestDto(
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    bool IsPremium,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsActive);
