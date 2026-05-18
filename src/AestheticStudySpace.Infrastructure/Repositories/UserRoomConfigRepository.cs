using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class UserRoomConfigRepository : IUserRoomConfigRepository
{
    private readonly AppDbContext _context;

    public UserRoomConfigRepository(AppDbContext context) => _context = context;

    public Task<UserRoomConfig?> GetByUserAndRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default) =>
        _context.UserRoomConfigs.FirstOrDefaultAsync(c => c.UserId == userId && c.RoomId == roomId, cancellationToken);

    public async Task<IReadOnlyList<UserRoomConfig>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.UserRoomConfigs
            .AsNoTracking()
            .Include(c => c.Room)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserRoomConfig config, CancellationToken cancellationToken = default) =>
        await _context.UserRoomConfigs.AddAsync(config, cancellationToken);

    public Task UpdateAsync(UserRoomConfig config, CancellationToken cancellationToken = default)
    {
        _context.UserRoomConfigs.Update(config);
        return Task.CompletedTask;
    }
}
