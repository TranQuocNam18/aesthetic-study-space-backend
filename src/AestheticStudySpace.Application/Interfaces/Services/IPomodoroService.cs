using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Pomodoro;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IPomodoroService
{
    Task<PomodoroSessionDto> StartAsync(Guid userId, StartPomodoroRequestDto request, CancellationToken cancellationToken = default);
    Task<PomodoroSessionDto> EndAsync(Guid userId, EndPomodoroRequestDto request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid userId, CancelPomodoroRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<PomodoroSessionDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PomodoroStatsDto> GetStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}
