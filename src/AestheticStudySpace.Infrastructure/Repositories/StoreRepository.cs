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

    public async Task<int> CountActiveItemsAsync(StoreCategory? category, StoreCatalogScope scope, CancellationToken cancellationToken = default) =>
        await ApplyCatalogFilter(_context.StoreItems.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted), category, scope)
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyList<StoreItem>> GetActiveItemsAsync(
        StoreCategory? category,
        StoreCatalogScope scope,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        return await ApplyCatalogFilter(_context.StoreItems.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted), category, scope)
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<StoreItem?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted, cancellationToken);

    public Task<StoreItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StoreItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<HashSet<Guid>> GetOwnedStoreItemIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await _context.UserInventories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.StoreItemId)
            .ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public Task<int> CountInventoryAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.UserInventories.AsNoTracking().CountAsync(x => x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<UserInventory>> GetInventoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        return await _context.UserInventories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.StoreItem)
            .OrderByDescending(x => x.AcquiredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<StoreItem> ApplyCatalogFilter(
        IQueryable<StoreItem> query,
        StoreCategory? category,
        StoreCatalogScope scope)
    {
        if (category is not null)
            query = query.Where(x => x.Category == category);
        else
        {
            query = scope switch
            {
                StoreCatalogScope.ThemesOnly => query.Where(x => x.Category == StoreCategory.Theme),
                StoreCatalogScope.AssetsOnly => query.Where(x => x.Category != StoreCategory.Theme),
                _ => query
            };
        }

        return query;
    }

    public async Task AddPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default) =>
        await _context.Purchases.AddAsync(purchase, cancellationToken);

    public async Task AddInventoryAsync(UserInventory inventory, CancellationToken cancellationToken = default) =>
        await _context.UserInventories.AddAsync(inventory, cancellationToken);

    public Task<bool> HasInventoryAsync(Guid userId, Guid storeItemId, CancellationToken cancellationToken = default) =>
        _context.UserInventories.AnyAsync(x => x.UserId == userId && x.StoreItemId == storeItemId, cancellationToken);

    public Task<bool> HasPurchaseForPaymentAsync(Guid paymentTransactionId, CancellationToken cancellationToken = default) =>
        _context.Purchases.AnyAsync(x => x.PaymentTransactionId == paymentTransactionId && !x.IsDeleted, cancellationToken);

    public async Task<int> CountPurchaseHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var index = await GetPurchaseHistoryIndexAsync(userId, cancellationToken);
        return index.Count;
    }

    public async Task<IReadOnlyList<(Guid Id, DateTime CreatedAt, bool IsPurchase)>> GetPurchaseHistoryIndexAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var purchases = await _context.Purchases
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => new { x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        var subscriptionPayments = await _context.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                !x.IsDeleted &&
                x.Purpose == PaymentPurpose.Subscription &&
                x.Status == PaymentStatus.Succeeded &&
                !_context.Purchases.Any(p => p.PaymentTransactionId == x.Id && !p.IsDeleted))
            .Select(x => new { x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        return purchases
            .Select(x => (x.Id, x.CreatedAt, IsPurchase: true))
            .Concat(subscriptionPayments.Select(x => (x.Id, x.CreatedAt, IsPurchase: false)))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<IReadOnlyList<Purchase>> GetPurchaseHistoryPurchasesAsync(
        Guid userId,
        IReadOnlyList<Guid> purchaseIds,
        CancellationToken cancellationToken = default)
    {
        if (purchaseIds.Count == 0)
            return Array.Empty<Purchase>();

        return await _context.Purchases
            .AsNoTracking()
            .Where(x => x.UserId == userId && purchaseIds.Contains(x.Id) && !x.IsDeleted)
            .Include(x => x.StoreItem)
            .Include(x => x.PaymentTransaction)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentTransaction>> GetPurchaseHistorySubscriptionPaymentsAsync(
        Guid userId,
        IReadOnlyList<Guid> paymentIds,
        CancellationToken cancellationToken = default)
    {
        if (paymentIds.Count == 0)
            return Array.Empty<PaymentTransaction>();

        return await _context.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                paymentIds.Contains(x.Id) &&
                !x.IsDeleted &&
                x.Purpose == PaymentPurpose.Subscription &&
                x.Status == PaymentStatus.Succeeded)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAllItemsAsync(StoreCategory? category, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _context.StoreItems.AsNoTracking().AsQueryable();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);
        if (category is not null)
            query = query.Where(x => x.Category == category);
        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoreItem>> GetAllItemsAsync(
        StoreCategory? category,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _context.StoreItems.AsNoTracking().AsQueryable();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);
        if (category is not null)
            query = query.Where(x => x.Category == category);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default) =>
        await _context.StoreItems.AddAsync(item, cancellationToken);

    public Task UpdateStoreItemAsync(StoreItem item, CancellationToken cancellationToken = default)
    {
        _context.StoreItems.Update(item);
        return Task.CompletedTask;
    }
}

