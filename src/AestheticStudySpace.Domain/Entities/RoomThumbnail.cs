namespace AestheticStudySpace.Domain.Entities;

public class RoomThumbnail : BaseEntity
{
    public Guid RoomLayoutId { get; set; }
    public RoomLayout RoomLayout { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string? PublicId { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
}

