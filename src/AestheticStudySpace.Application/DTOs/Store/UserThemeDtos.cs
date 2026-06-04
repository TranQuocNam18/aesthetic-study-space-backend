using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

/// <summary>Request DTO when a user submits a new theme for review.</summary>
public record SubmitThemeRequestDto(
    string Name,
    string? Description,
    /// <summary>Cloudinary URL of the theme asset (already uploaded by client).</summary>
    string AssetUrl,
    int? CoinPrice,
    long? RealMoneyPriceVnd);

/// <summary>Response DTO showing one of the user's submitted themes.</summary>
public record UserThemeSubmissionDto(
    Guid Id,
    string Name,
    string? Description,
    string AssetUrl,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    StoreItemStatus Status,
    string? RejectionNote,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);
