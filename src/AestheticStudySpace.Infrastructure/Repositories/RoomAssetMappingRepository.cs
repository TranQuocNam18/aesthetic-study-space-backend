using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class RoomAssetMappingRepository : IRoomAssetMappingRepository
{
    private readonly AppDbContext _context;

    public RoomAssetMappingRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<RoomAssetMapping>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default) =>
        await _context.RoomAssetMappings
            .AsNoTracking()
            .Include(m => m.Asset)
            .Where(m => m.RoomId == roomId)
            .OrderBy(m => m.DefaultLayerIndex)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(RoomAssetMapping mapping, CancellationToken cancellationToken = default) =>
        await _context.RoomAssetMappings.AddAsync(mapping, cancellationToken);

    public async Task DeleteByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default) =>
        await _context.RoomAssetMappings.Where(m => m.RoomId == roomId).ExecuteDeleteAsync(cancellationToken);
}
