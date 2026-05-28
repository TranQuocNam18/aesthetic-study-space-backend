using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class UserMissionRepository : IUserMissionRepository
{
    private readonly AppDbContext _context;

    public UserMissionRepository(AppDbContext context) => _context = context;

    public Task<UserMission?> GetForPeriodAsync(Guid userId, Guid missionId, DateOnly periodDate, CancellationToken cancellationToken = default) =>
        _context.UserMissions.FirstOrDefaultAsync(x => x.UserId == userId && x.MissionId == missionId && x.PeriodDate == periodDate, cancellationToken);

    public async Task AddAsync(UserMission userMission, CancellationToken cancellationToken = default) =>
        await _context.UserMissions.AddAsync(userMission, cancellationToken);

    public Task UpdateAsync(UserMission userMission, CancellationToken cancellationToken = default)
    {
        _context.UserMissions.Update(userMission);
        return Task.CompletedTask;
    }
}

