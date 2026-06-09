using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Coins;
using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface ICoinService
{
    Task<CoinBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<CoinTransactionDto>> GetTransactionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CoinTransactionType? type,
        CancellationToken cancellationToken = default);
}
