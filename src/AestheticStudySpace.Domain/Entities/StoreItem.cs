using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class StoreItem : BaseEntity
{
    public StoreCategory Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string AssetUrl { get; set; } = string.Empty;
    public bool IsPremium { get; set; }

    public int? CoinPrice { get; set; }
    public long? RealMoneyPriceVnd { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Who submitted this item. Null means it was created directly by an Admin.
    /// </summary>
    public Guid? CreatorId { get; set; }
    public User? Creator { get; set; }

    /// <summary>
    /// Review status. AdminCreated items are always approved.
    /// </summary>
    public StoreItemStatus Status { get; set; } = StoreItemStatus.AdminCreated;

    /// <summary>
    /// Admin note explaining why a user-submitted theme was rejected.
    /// </summary>
    public string? RejectionNote { get; set; }

    /// <summary>When Admin reviewed (approved or rejected) this submission.</summary>
    public DateTime? ReviewedAt { get; set; }
}

