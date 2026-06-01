namespace AestheticStudySpace.Domain.Entities;

public class UserInventory : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid StoreItemId { get; set; }
    public StoreItem StoreItem { get; set; } = null!;

    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}

