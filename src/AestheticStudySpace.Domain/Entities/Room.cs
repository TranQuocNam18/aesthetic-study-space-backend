namespace AestheticStudySpace.Domain.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public bool IsPremium { get; set; }

    /// <summary>
    /// null = global room managed by Admin; non-null = custom room created by a User.
    /// </summary>
    public Guid? UserId { get; set; }
    public User? Owner { get; set; }

    public ICollection<RoomAssetMapping> AssetMappings { get; set; } = new List<RoomAssetMapping>();
    public ICollection<UserRoomConfig> UserConfigs { get; set; } = new List<UserRoomConfig>();
}
