using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Coins;

public record CoinBalanceDto(int Balance);

public record CoinTransactionDto(
    Guid Id,
    CoinTransactionType Type,
    int Amount,
    string Reason,
    Guid? RelatedPurchaseId,
    Guid? RelatedMissionId,
    DateTime CreatedAt);
