namespace AestheticStudySpace.Application.DTOs.Assets;

public record AssetDto(
    Guid Id,
    string Name,
    string? Description,
    string Url,
    string Type,
    string Category,
    int DefaultVolume,
    bool IsPremium);

public record CreateAssetRequestDto(
    string Name,
    string? Description,
    string Url,
    string Type,
    string Category,
    int DefaultVolume,
    bool IsPremium);

public record UpdateAssetRequestDto(
    string Name,
    string? Description,
    string Url,
    string Type,
    string Category,
    int DefaultVolume,
    bool IsPremium);

public record RoomAssetMappingRequestDto(
    Guid AssetId,
    double DefaultPositionX,
    double DefaultPositionY,
    double DefaultScale,
    double DefaultOpacity,
    int DefaultLayerIndex);
