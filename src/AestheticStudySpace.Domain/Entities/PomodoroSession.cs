namespace AestheticStudySpace.Domain.Entities;

public class PomodoroSession : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
}
