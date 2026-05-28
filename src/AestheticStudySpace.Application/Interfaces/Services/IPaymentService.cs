using AestheticStudySpace.Application.DTOs.Payments;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<VnPayCreateResponseDto> CreateVnPayAsync(Guid userId, CreateVnPayPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task HandleVnPayCallbackAsync(IReadOnlyDictionary<string, string> query, CancellationToken cancellationToken = default);

    Task<SePayCreateResponseDto> CreateSePayAsync(Guid userId, CreateSePayPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task HandleSePayWebhookAsync(string rawBody, string? signature, CancellationToken cancellationToken = default);
}

