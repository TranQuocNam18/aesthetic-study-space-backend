namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IRetentionEmailService
{
    Task<int> SendRetentionEmailsAsync(CancellationToken cancellationToken = default);
    Task<bool> SendRetentionEmailToUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
