namespace AestheticStudySpace.Application.DTOs.Payments;

// ── VNPay ──────────────────────────────────────────────────────────────────
public record CreateVnPayPaymentRequestDto(long AmountVnd, string? ReturnUrl, string? Description, string Purpose, Guid? StoreItemId, int? CoinsAmount);
public record VnPayCreateResponseDto(string TransactionCode, string PaymentUrl);
public record SubscriptionUpgradeRequestDto(string TransactionCode);

// ── PayOS ──────────────────────────────────────────────────────────────────
public record CreatePayOsPaymentRequestDto(
    long   AmountVnd,
    string? Description,
    string  Purpose,
    string? ReturnUrl,
    string? CancelUrl,
    Guid?   StoreItemId,
    int?    CoinsAmount);

public record PayOsCreateResponseDto(string TransactionCode, string CheckoutUrl);


