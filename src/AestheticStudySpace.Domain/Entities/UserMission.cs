namespace AestheticStudySpace.Domain.Entities;

public class UserMission : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public int ProgressValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }

    /// <summary>
    /// Typically truncated to date for daily/weekly missions.
    /// </summary>
    public DateOnly PeriodDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

