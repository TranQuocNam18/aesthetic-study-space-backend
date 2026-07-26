using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IUserMissionRepository
{
    Task<UserMission?> GetForPeriodAsync(Guid userId, Guid missionId, DateOnly periodDate, CancellationToken cancellationToken = default);
    Task<UserMission?> GetLatestForMissionAsync(Guid userId, Guid missionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserMission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteOlderThanAsync(DateOnly beforeDate, CancellationToken cancellationToken = default);
    Task AddAsync(UserMission userMission, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserMission userMission, CancellationToken cancellationToken = default);
}

