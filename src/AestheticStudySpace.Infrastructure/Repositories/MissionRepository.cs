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
        await _context.Missions.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Missions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Mission>> GetActiveByTriggerKeyAsync(string triggerKey, CancellationToken cancellationToken = default) =>
        await _context.Missions.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted && x.TriggerKey == triggerKey)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Mission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _context.Missions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<int> CountAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _context.Missions.AsNoTracking().AsQueryable();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);
        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Mission>> GetAllAsync(bool includeInactive, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _context.Missions.AsNoTracking().AsQueryable();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Mission mission, CancellationToken cancellationToken = default) =>
        await _context.Missions.AddAsync(mission, cancellationToken);

    public Task UpdateAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        _context.Missions.Update(mission);
        return Task.CompletedTask;
    }
}

