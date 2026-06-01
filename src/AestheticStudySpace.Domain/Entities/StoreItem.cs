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
}

