using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly AppDbContext _context;

    public StoreRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<StoreItem>> GetActiveItemsAsync(StoreCategory? category, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        var query = _context.StoreItems.AsNoTracking().Where(x => x.IsActive);
        if (category is not null)
            query = query.Where(x => x.Category == category);

        return await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<StoreItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default) =>
        await _context.Purchases.AddAsync(purchase, cancellationToken);

    public async Task AddInventoryAsync(UserInventory inventory, CancellationToken cancellationToken = default) =>
        await _context.UserInventories.AddAsync(inventory, cancellationToken);

    public Task<bool> HasInventoryAsync(Guid userId, Guid storeItemId, CancellationToken cancellationToken = default) =>
        _context.UserInventories.AnyAsync(x => x.UserId == userId && x.StoreItemId == storeItemId, cancellationToken);
}

