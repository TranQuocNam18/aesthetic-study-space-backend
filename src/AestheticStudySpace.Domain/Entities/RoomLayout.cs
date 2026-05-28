namespace AestheticStudySpace.Domain.Entities;

public class RoomLayout : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }

    public string LayoutJson { get; set; } = "{}";
    public string? ThumbnailUrl { get; set; }
}

