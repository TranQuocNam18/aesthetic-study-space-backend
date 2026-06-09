using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class CoinTransactionRepository : ICoinTransactionRepository
{
    private readonly AppDbContext _context;

    public CoinTransactionRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(CoinTransaction tx, CancellationToken cancellationToken = default) =>
        await _context.CoinTransactions.AddAsync(tx, cancellationToken);

    public Task<int> CountByUserAsync(Guid userId, CoinTransactionType? type, CancellationToken cancellationToken = default)
    {
        var query = _context.CoinTransactions.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted);
        if (type is not null)
            query = query.Where(x => x.Type == type);
        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CoinTransaction>> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CoinTransactionType? type,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 20 : pageSize;

        var query = _context.CoinTransactions.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted);
        if (type is not null)
            query = query.Where(x => x.Type == type);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}

