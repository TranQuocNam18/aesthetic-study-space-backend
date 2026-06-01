using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Repositories;

public interface ICoinTransactionRepository
{
    Task AddAsync(CoinTransaction tx, CancellationToken cancellationToken = default);
}

