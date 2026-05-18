using AestheticStudySpace.Application.DTOs.Workspace;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IWorkspaceService
{
    Task<IReadOnlyList<WorkspaceConfigDto>> GetMyWorkspaceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkspaceConfigDto> SaveAsync(Guid userId, SaveWorkspaceRequestDto request, CancellationToken cancellationToken = default);
}
