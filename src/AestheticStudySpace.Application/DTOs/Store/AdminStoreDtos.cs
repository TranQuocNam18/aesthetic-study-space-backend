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
    StoreItemStatus Status,
    Guid? CreatorId,
    string? CreatorUsername,
    string? RejectionNote,
    DateTime? ReviewedAt,
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

public record ApproveThemeRequestDto(
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    bool IsPremium = false);

public record RejectThemeRequestDto(string RejectionNote);
