using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class CoinTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public CoinTransactionType Type { get; set; }
    public int Amount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Guid? RelatedMissionId { get; set; }
    public Mission? RelatedMission { get; set; }

    public Guid? RelatedPurchaseId { get; set; }
    public Purchase? RelatedPurchase { get; set; }
}

