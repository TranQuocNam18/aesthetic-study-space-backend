using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentPurpose Purpose { get; set; } = PaymentPurpose.Subscription;

    public string TransactionCode { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "VND";

    public string? ProviderPayloadJson { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsFulfilled { get; set; }

    public DateTime? SucceededAt { get; set; }
    public DateTime? FailedAt { get; set; }
}

