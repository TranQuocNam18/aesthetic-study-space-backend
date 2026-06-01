using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Coins;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class CoinService : ICoinService
{
    private readonly IUserRepository _userRepository;
    private readonly ICoinTransactionRepository _coinTransactionRepository;

    public CoinService(IUserRepository userRepository, ICoinTransactionRepository coinTransactionRepository)
    {
        _userRepository = userRepository;
        _coinTransactionRepository = coinTransactionRepository;
    }

    public async Task<CoinBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new CoinBalanceDto(user.CoinsBalance);
    }

    public async Task<PagedResult<CoinTransactionDto>> GetTransactionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CoinTransactionType? type,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _coinTransactionRepository.CountByUserAsync(userId, type, cancellationToken);
        var transactions = await _coinTransactionRepository.GetByUserAsync(userId, page, pageSize, type, cancellationToken);

        return new PagedResult<CoinTransactionDto>
        {
            Items = transactions.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private static CoinTransactionDto ToDto(Domain.Entities.CoinTransaction tx) =>
        new(tx.Id, tx.Type, tx.Amount, tx.Reason, tx.RelatedPurchaseId, tx.RelatedMissionId, tx.CreatedAt);
}
