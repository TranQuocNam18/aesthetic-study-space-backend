using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class Asset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public AssetCategory Category { get; set; }
    public int DefaultVolume { get; set; } = 70;
    public bool IsPremium { get; set; }

    public ICollection<RoomAssetMapping> RoomMappings { get; set; } = new List<RoomAssetMapping>();
}
