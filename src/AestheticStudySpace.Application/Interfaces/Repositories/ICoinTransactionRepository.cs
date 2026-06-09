using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface ICoinTransactionRepository
{
    Task AddAsync(CoinTransaction tx, CancellationToken cancellationToken = default);
    Task<int> CountByUserAsync(Guid userId, CoinTransactionType? type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CoinTransaction>> GetByUserAsync(Guid userId, int page, int pageSize, CoinTransactionType? type, CancellationToken cancellationToken = default);
}

