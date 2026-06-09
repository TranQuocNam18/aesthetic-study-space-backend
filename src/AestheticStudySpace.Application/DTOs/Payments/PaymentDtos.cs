namespace AestheticStudySpace.Application.DTOs.Payments;

public record CreateVnPayPaymentRequestDto(long AmountVnd, string? ReturnUrl, string? Description, string Purpose, Guid? StoreItemId, int? CoinsAmount);
public record VnPayCreateResponseDto(string TransactionCode, string PaymentUrl);
public record SubscriptionUpgradeRequestDto(string TransactionCode);

