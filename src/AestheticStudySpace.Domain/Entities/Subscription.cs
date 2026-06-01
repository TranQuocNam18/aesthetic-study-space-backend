namespace AestheticStudySpace.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public bool IsActive { get; set; }

    public Guid? PaymentTransactionId { get; set; }
    public PaymentTransaction? PaymentTransaction { get; set; }
}

