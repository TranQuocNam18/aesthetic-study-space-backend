namespace AestheticStudySpace.Application.DTOs.RoomLayouts;

public record SaveRoomLayoutRequestDto(
    string Name,
    string? Description,
    Guid? RoomId,
    string LayoutJson,
    string? ThumbnailBase64Png);

public record RoomLayoutDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? RoomId,
    string LayoutJson,
    string? ThumbnailUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

