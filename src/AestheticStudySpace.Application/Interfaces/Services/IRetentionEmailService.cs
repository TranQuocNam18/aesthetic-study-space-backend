namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IRetentionEmailService
{
    Task<int> SendRetentionEmailsAsync(CancellationToken cancellationToken = default);
}
