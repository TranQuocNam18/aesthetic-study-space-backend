namespace AestheticStudySpace.Domain.Entities;

public class RoomAssetMapping : BaseEntity
{
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
    public double DefaultPositionX { get; set; }
    public double DefaultPositionY { get; set; }
    public double DefaultScale { get; set; } = 1.0;
    public double DefaultOpacity { get; set; } = 1.0;
    public int DefaultLayerIndex { get; set; }
}
