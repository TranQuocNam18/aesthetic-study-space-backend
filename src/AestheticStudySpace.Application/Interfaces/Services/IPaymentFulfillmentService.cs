using AestheticStudySpace.Domain.Entities;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IPaymentFulfillmentService
{
    Task FulfillIfNeededAsync(PaymentTransaction tx, CancellationToken cancellationToken = default);
}

