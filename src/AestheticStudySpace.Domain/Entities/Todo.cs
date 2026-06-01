namespace AestheticStudySpace.Domain.Entities;

public class Todo : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
