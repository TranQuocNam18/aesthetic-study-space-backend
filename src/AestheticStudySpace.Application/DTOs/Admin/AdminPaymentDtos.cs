using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Application.DTOs.Admin;

public record AdminPaymentTransactionDto(
    Guid Id,
    Guid UserId,
    string Username,
    string Email,
    PaymentProvider Provider,
    PaymentStatus Status,
    PaymentPurpose Purpose,
    string TransactionCode,
    long Amount,
    string Currency,
    string? ProviderPayloadJson,
    string? MetadataJson,
    bool IsFulfilled,
    DateTime? SucceededAt,
    DateTime? FailedAt,
    DateTime CreatedAt);
