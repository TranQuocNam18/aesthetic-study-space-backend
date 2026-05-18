namespace AestheticStudySpace.Application.DTOs.Workspace;

public record WorkspaceConfigDto(
    Guid? ConfigId,
    Guid RoomId,
    string RoomName,
    string JsonConfig,
    DateTime? UpdatedAt);

public record SaveWorkspaceRequestDto(Guid RoomId, string JsonConfig);
