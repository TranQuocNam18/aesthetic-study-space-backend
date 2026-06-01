using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class RoomLayoutRepository : IRoomLayoutRepository
{
    private readonly AppDbContext _context;

    public RoomLayoutRepository(AppDbContext context) => _context = context;

    public Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.RoomLayouts.CountAsync(x => x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<RoomLayout>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        return await _context.RoomLayouts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<RoomLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.RoomLayouts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(RoomLayout layout, CancellationToken cancellationToken = default) =>
        await _context.RoomLayouts.AddAsync(layout, cancellationToken);

    public Task UpdateAsync(RoomLayout layout, CancellationToken cancellationToken = default)
    {
        _context.RoomLayouts.Update(layout);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RoomLayout layout, CancellationToken cancellationToken = default)
    {
        _context.RoomLayouts.Remove(layout);
        return Task.CompletedTask;
    }
}

