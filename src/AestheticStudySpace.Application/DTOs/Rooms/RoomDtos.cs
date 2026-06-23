namespace AestheticStudySpace.Application.DTOs.Rooms;

public record RoomListItemDto(
    Guid Id,
    string Name,
    string? Description,
    string? ThumbnailUrl,
    bool IsPremium);

public record RoomAssetDto(
    Guid Id,
    string Name,
    string Type,
    string Category,
    string Url,
    int DefaultVolume,
    bool IsPremium,
    string? PreviewUrl,
    double DefaultPositionX,
    double DefaultPositionY,
    double DefaultScale,
    double DefaultOpacity,
    int DefaultLayerIndex);

public record RoomDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string? BackgroundUrl,
    bool IsPremium,
    IReadOnlyList<RoomAssetDto> Assets);

public record CreateRoomRequestDto(
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string? BackgroundUrl,
    bool IsPremium);

public record UpdateRoomRequestDto(
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string? BackgroundUrl,
    bool IsPremium);

/// <summary>DTO used by authenticated users to create their own custom room.</summary>
public record UserCreateRoomRequestDto(
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string? BackgroundUrl);

/// <summary>DTO used by authenticated users to update their own custom room.</summary>
public record UserUpdateRoomRequestDto(
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string? BackgroundUrl);
