namespace AestheticStudySpace.Application.DTOs.Store;

public record AdminApproveTransactionDto(
    bool PayInCoins,
    string? TransactionNote = null);

public record AdminPriceAndPublishDto(
    int CoinPrice,
    bool IsPremium);
