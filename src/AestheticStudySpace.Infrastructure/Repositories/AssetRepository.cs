using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Asset>> GetAllAsync(
        AssetType? type = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets.AsNoTracking().AsQueryable();

        if (type.HasValue)
            query = query.Where(a => a.AssetType == type.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category.ToLower() == category.ToLower());

        return await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Assets.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default) =>
        await _context.Assets.AddAsync(asset, cancellationToken);

    public Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _context.Assets.Update(asset);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _context.Assets.Remove(asset);
        return Task.CompletedTask;
    }
}
