using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

/// <summary>
/// Represents a self-designed component (Sticker / Background / Effect / AmbientSound)
/// that is uploaded inline as part of a mixed Theme combo submission.
/// The asset must already be uploaded to Cloudinary before submission.
/// </summary>
public record InlineComponentDto(
    /// <summary>Must be Sticker, Background, Effect, or AmbientSound. NOT Theme.</summary>
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl);

/// <summary>
/// Request DTO when a user submits a new theme for review.
/// Supports three slot modes for each component type (Sticker / Background / Effect / AmbientSound):
///   1. ThemeXxxItemId set   → reuse an existing approved Store item.
///   2. InlineXxx set        → user uploads a new custom component as part of this Theme combo.
///   3. Both null            → slot is empty (total filled slots must be ≥ 2).
/// IMPORTANT: You must NOT provide both ThemeXxxItemId and InlineXxx for the same slot.
/// </summary>
public record SubmitThemeRequestDto(
    string Name,
    string? Description,
    /// <summary>Cloudinary URL of the overall theme preview asset (already uploaded by client).</summary>
    string AssetUrl,
    string? PreviewUrl,
    // ── Existing store items (by ID) ─────────────────────────────────────────
    Guid? ThemeStickerItemId,
    Guid? ThemeBackgroundItemId,
    Guid? ThemeEffectItemId,
    Guid? ThemeAmbientSoundItemId,
    // ── Inline (self-uploaded) components ───────────────────────────────────
    InlineComponentDto? InlineSticker,
    InlineComponentDto? InlineBackground,
    InlineComponentDto? InlineEffect,
    InlineComponentDto? InlineAmbientSound,
    // ── Pricing ──────────────────────────────────────────────────────────────
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    List<Guid>? ThemeStickerItemIds = null,
    List<Guid>? ThemeBackgroundItemIds = null,
    List<Guid>? ThemeEffectItemIds = null,
    List<Guid>? ThemeAmbientSoundItemIds = null,
    List<InlineComponentDto>? InlineStickers = null,
    List<InlineComponentDto>? InlineBackgrounds = null,
    List<InlineComponentDto>? InlineEffects = null,
    List<InlineComponentDto>? InlineAmbientSounds = null);

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
    DateTime? ReviewedAt,
    /// <summary>Inline components that were created as part of this Theme combo (Luồng C).</summary>
    IReadOnlyList<UserComponentSubmissionDto> InlineComponents);

/// <summary>Request DTO for partially updating a user's theme submission.</summary>
public record PatchThemeRequestDto(
    string? Name = null,
    string? Description = null,
    string? AssetUrl = null,
    string? PreviewUrl = null,
    // ── Existing store items ──────────────────────────────────────────────────
    Guid? ThemeStickerItemId = null,
    Guid? ThemeBackgroundItemId = null,
    Guid? ThemeEffectItemId = null,
    Guid? ThemeAmbientSoundItemId = null,
    List<Guid>? ThemeStickerItemIds = null,
    List<Guid>? ThemeBackgroundItemIds = null,
    List<Guid>? ThemeEffectItemIds = null,
    List<Guid>? ThemeAmbientSoundItemIds = null,
    // ── Inline components ─────────────────────────────────────────────────────
    InlineComponentDto? InlineSticker = null,
    InlineComponentDto? InlineBackground = null,
    InlineComponentDto? InlineEffect = null,
    InlineComponentDto? InlineAmbientSound = null,
    List<InlineComponentDto>? InlineStickers = null,
    List<InlineComponentDto>? InlineBackgrounds = null,
    List<InlineComponentDto>? InlineEffects = null,
    List<InlineComponentDto>? InlineAmbientSounds = null,
    // ── Pricing ───────────────────────────────────────────────────────────────
    int? CoinPrice = null,
    long? RealMoneyPriceVnd = null);
