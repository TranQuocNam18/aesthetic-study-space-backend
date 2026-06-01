using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Infrastructure.Persistence;

namespace AestheticStudySpace.Infrastructure.Repositories;

public class CoinTransactionRepository : ICoinTransactionRepository
{
    private readonly AppDbContext _context;

    public CoinTransactionRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(CoinTransaction tx, CancellationToken cancellationToken = default) =>
        await _context.CoinTransactions.AddAsync(tx, cancellationToken);
}

