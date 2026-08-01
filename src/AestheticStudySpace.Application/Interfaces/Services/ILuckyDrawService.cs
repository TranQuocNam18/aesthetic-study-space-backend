using AestheticStudySpace.Application.DTOs.LuckyDraw;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface ILuckyDrawService
{
    Task<LuckyDrawStatusDto> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LuckyDrawResultDto> SpinAsync(Guid userId, CancellationToken cancellationToken = default);
}
