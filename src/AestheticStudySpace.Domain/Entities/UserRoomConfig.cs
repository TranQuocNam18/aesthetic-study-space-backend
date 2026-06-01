namespace AestheticStudySpace.Domain.Entities;

public class UserRoomConfig : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public string JsonConfig { get; set; } = "{}";
}
