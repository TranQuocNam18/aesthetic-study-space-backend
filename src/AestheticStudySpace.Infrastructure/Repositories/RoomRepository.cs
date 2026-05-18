using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Rooms.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public Task<Room?> GetByIdAsync(Guid id, bool includeAssets = false, CancellationToken cancellationToken = default) =>
        _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(Room room, CancellationToken cancellationToken = default) =>
        await _context.Rooms.AddAsync(room, cancellationToken);

    public Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(room);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Remove(room);
        return Task.CompletedTask;
    }
}
