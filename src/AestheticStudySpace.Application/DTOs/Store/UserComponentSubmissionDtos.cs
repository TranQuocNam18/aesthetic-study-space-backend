using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Store;

/// <summary>
/// Request DTO when a user submits a self-designed component (Sticker, Background, Effect, or AmbientSound)
/// for admin review. The AssetUrl must already be uploaded to Cloudinary by the client.
/// Category must NOT be Theme — use the theme submission endpoints for that.
/// </summary>
public record SubmitComponentRequestDto(
    StoreCategory Category,
    string Name,
    string? Description,
    /// <summary>Cloudinary URL of the asset (already uploaded by client).</summary>
    string AssetUrl,
    string? PreviewUrl,
    string? BankAccountNumber = null,
    string? BankName = null,
    string? BankAccountOwnerName = null,
    int? RequestedCoinPrice = null,
    long? RequestedRealMoneyPriceVnd = null,
    bool IsAgreedToTerms = false);

/// <summary>Response DTO showing one of the user's submitted components.</summary>
public record UserComponentSubmissionDto(
    Guid Id,
    StoreCategory Category,
    string Name,
    string? Description,
    string AssetUrl,
    string? PreviewUrl,
    int? CoinPrice,
    long? RealMoneyPriceVnd,
    StoreItemStatus Status,
    string? RejectionNote,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? BankAccountNumber = null,
    string? BankName = null,
    string? BankAccountOwnerName = null,
    int? RequestedCoinPrice = null,
    long? RequestedRealMoneyPriceVnd = null,
    bool IsBoughtByAdmin = false);

/// <summary>Request DTO for partially updating a user's component submission.</summary>
public record PatchComponentRequestDto(
    string? Name = null,
    string? Description = null,
    string? AssetUrl = null,
    string? PreviewUrl = null,
    string? BankAccountNumber = null,
    string? BankName = null,
    string? BankAccountOwnerName = null,
    int? RequestedCoinPrice = null,
    long? RequestedRealMoneyPriceVnd = null);
