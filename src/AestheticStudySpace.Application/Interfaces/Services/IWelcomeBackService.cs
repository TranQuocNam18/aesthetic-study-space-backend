using AestheticStudySpace.Application.DTOs.Workspace;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IWelcomeBackService
{
    Task<WelcomeBackDto> GetWelcomeBackMessageAsync(Guid userId, CancellationToken cancellationToken = default);
}
