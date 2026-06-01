using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class MissionRepository : IMissionRepository
{
    private readonly AppDbContext _context;

    public MissionRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Mission>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _context.Missions.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Missions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Mission>> GetActiveByTriggerKeyAsync(string triggerKey, CancellationToken cancellationToken = default) =>
        await _context.Missions.AsNoTracking()
            .Where(x => x.IsActive && x.TriggerKey == triggerKey)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}

