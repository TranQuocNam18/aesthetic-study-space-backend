using AestheticStudySpace.Application.DTOs.Assets;
using AestheticStudySpace.Application.DTOs.Auth;
using AestheticStudySpace.Application.DTOs.Pomodoro;
using AestheticStudySpace.Application.DTOs.Rooms;
using AestheticStudySpace.Application.DTOs.Todos;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Mapping;

public static class MappingExtensions
{
    public static UserProfileDto ToProfileDto(this User user) =>
        new(user.Id, user.Username, user.Email, user.Role.ToString(), user.AccountTier.ToString(), user.AvatarUrl, user.CreatedAt);

    public static RoomListItemDto ToListItemDto(this Room room) =>
        new(room.Id, room.Name, room.Description, room.ThumbnailUrl, room.IsPremium);

    public static RoomAssetDto ToRoomAssetDto(this RoomAssetMapping mapping) =>
        new(
            mapping.Asset.Id,
            mapping.Asset.Name,
            mapping.Asset.AssetType.ToString(),
            mapping.Asset.Category.ToString(),
            mapping.Asset.Url,
            mapping.Asset.DefaultVolume,
            mapping.Asset.IsPremium,
            mapping.DefaultPositionX,
            mapping.DefaultPositionY,
            mapping.DefaultScale,
            mapping.DefaultOpacity,
            mapping.DefaultLayerIndex);

    public static RoomDetailDto ToDetailDto(this Room room, IReadOnlyList<RoomAssetMapping> mappings) =>
        new(
            room.Id,
            room.Name,
            room.Description,
            room.ThumbnailUrl,
            room.BackgroundUrl,
            room.IsPremium,
            mappings.Select(m => m.ToRoomAssetDto()).ToList());

    public static AssetDto ToDto(this Asset asset) =>
        new(asset.Id, asset.Name, asset.Description, asset.Url, asset.AssetType.ToString(), asset.Category.ToString(), asset.DefaultVolume, asset.IsPremium);

    public static TodoDto ToDto(this Todo todo) =>
        new(todo.Id, todo.Content, todo.IsCompleted, todo.CreatedAt);

    public static PomodoroSessionDto ToDto(this PomodoroSession session) =>
        new(session.Id, session.StartTime, session.EndTime, session.DurationMinutes, session.EndTime is null);

    public static AssetType ParseAssetType(string type) =>
        Enum.TryParse<AssetType>(type, true, out var result) ? result : throw new Domain.Exceptions.ValidationException($"Invalid asset type: {type}");

    public static AssetCategory ParseAssetCategory(string category) =>
        Enum.TryParse<AssetCategory>(category, true, out var result) ? result : throw new Domain.Exceptions.ValidationException($"Invalid asset category: {category}");
}
