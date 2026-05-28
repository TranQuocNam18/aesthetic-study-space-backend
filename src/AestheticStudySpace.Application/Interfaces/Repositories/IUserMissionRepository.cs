using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface IUserMissionRepository
{
    Task<UserMission?> GetForPeriodAsync(Guid userId, Guid missionId, DateOnly periodDate, CancellationToken cancellationToken = default);
    Task AddAsync(UserMission userMission, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserMission userMission, CancellationToken cancellationToken = default);
}

