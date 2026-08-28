using AestheticStudySpace.Application.DTOs.Payments;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IPaymentService
{
    // ── VNPay ────────────────────────────────────────────────────────────────
    Task<VnPayCreateResponseDto> CreateVnPayAsync(Guid userId, CreateVnPayPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task HandleVnPayCallbackAsync(IReadOnlyDictionary<string, string> query, CancellationToken cancellationToken = default);

    // ── PayOS ────────────────────────────────────────────────────────────────
    Task<PayOsCreateResponseDto> CreatePayOsAsync(Guid userId, CreatePayOsPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task HandlePayOsWebhookAsync(string webhookBody, CancellationToken cancellationToken = default);
}


