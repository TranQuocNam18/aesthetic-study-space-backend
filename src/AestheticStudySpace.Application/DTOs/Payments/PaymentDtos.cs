namespace AestheticStudySpace.Application.DTOs.Payments;

public record CreateVnPayPaymentRequestDto(long AmountVnd, string ReturnUrl, string? Description, string Purpose, Guid? StoreItemId, int? CoinsAmount);
public record VnPayCreateResponseDto(string TransactionCode, string PaymentUrl);

public record CreateSePayPaymentRequestDto(long AmountVnd, string? Description, string Purpose, Guid? StoreItemId, int? CoinsAmount);
public record SePayCreateResponseDto(string TransactionCode);

public record SubscriptionUpgradeRequestDto(string TransactionCode);

