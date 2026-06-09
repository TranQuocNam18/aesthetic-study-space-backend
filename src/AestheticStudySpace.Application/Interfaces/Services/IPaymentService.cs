using AestheticStudySpace.Application.DTOs.Payments;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<VnPayCreateResponseDto> CreateVnPayAsync(Guid userId, CreateVnPayPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task HandleVnPayCallbackAsync(IReadOnlyDictionary<string, string> query, CancellationToken cancellationToken = default);
}

