using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

/// <summary>Request DTO when a user submits a new theme for review.</summary>
public record SubmitThemeRequestDto(
    string Name,
    string? Description,
    /// <summary>Cloudinary URL of the theme asset (already uploaded by client).</summary>
    string AssetUrl,
    string? PreviewUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    int? CoinPrice,
    long? RealMoneyPriceVnd);

/// <summary>Response DTO showing one of the user's submitted themes.</summary>
public record UserThemeSubmissionDto(
    Guid Id,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl,
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    StoreThemeSource ThemeSource,
    StoreItemStatus Status,
    string? RejectionNote,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);

/// <summary>Request DTO for partially updating a user's theme submission.</summary>
public record PatchThemeRequestDto(
    string? Name = null,
    string? Description = null,
    string? AssetUrl = null,
    string? PreviewUrl = null,
    Guid? ThemeStickerItemId = null,
    Guid? ThemeBackgroundItemId = null,
    Guid? ThemeEffectItemId = null,
    Guid? ThemeAmbientSoundItemId = null,
    int? CoinPrice = null,
    long? RealMoneyPriceVnd = null);

