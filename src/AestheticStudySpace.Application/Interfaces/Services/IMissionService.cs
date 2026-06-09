using AestheticStudySpace.Application.DTOs.Missions;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IMissionService
{
    Task<IReadOnlyList<MissionDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MissionWithProgressDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserMissionDto> IncrementAsync(Guid userId, Guid missionId, int delta, CancellationToken cancellationToken = default);
    Task<UserMissionDto> ClaimAsync(Guid userId, Guid missionId, CancellationToken cancellationToken = default);
    Task IncrementByTriggerKeyAsync(Guid userId, string triggerKey, int delta, CancellationToken cancellationToken = default);
}

