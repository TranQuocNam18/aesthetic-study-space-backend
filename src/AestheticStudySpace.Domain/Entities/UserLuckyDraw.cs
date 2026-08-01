namespace AestheticStudySpace.Domain.Entities;

public class UserLuckyDraw : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly DrawDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public int RewardCoins { get; set; }
    public string RewardDescription { get; set; } = string.Empty;
}
