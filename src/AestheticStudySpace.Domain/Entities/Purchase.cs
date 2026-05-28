using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class Purchase : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? StoreItemId { get; set; }
    public StoreItem? StoreItem { get; set; }

    public int? CoinsSpent { get; set; }
    public long? AmountVnd { get; set; }

    public string Currency { get; set; } = "VND";

    public Guid? PaymentTransactionId { get; set; }
    public PaymentTransaction? PaymentTransaction { get; set; }
}

