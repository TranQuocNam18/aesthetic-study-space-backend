using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Pomodoro;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class PomodoroService : IPomodoroService
{
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly IMissionService _missionService;
    private readonly IUnitOfWork _unitOfWork;

    public PomodoroService(IPomodoroRepository pomodoroRepository, IMissionService missionService, IUnitOfWork unitOfWork)
    {
        _pomodoroRepository = pomodoroRepository;
        _missionService = missionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PomodoroSessionDto> StartAsync(Guid userId, StartPomodoroRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.DurationMinutes is < 1 or > 120)
            throw new ValidationException("Duration must be between 1 and 120 minutes.");

        var active = await _pomodoroRepository.GetActiveSessionAsync(userId, cancellationToken);
        if (active is not null)
        {
            // Auto-expire: nếu session cũ đã quá thời gian (FE bị tắt đột ngột mà không cancel)
            // thì xóa nó để cho phép tạo session mới.
            var expectedEnd = active.StartTime.AddMinutes(active.DurationMinutes);
            if (DateTime.UtcNow > expectedEnd)
            {
                await _pomodoroRepository.DeleteAsync(active, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new ValidationException("An active Pomodoro session already exists. End it before starting a new one.");
            }
        }

        var session = new PomodoroSession
        {
            UserId = userId,
            StartTime = DateTime.UtcNow,
            DurationMinutes = request.DurationMinutes
        };

        await _pomodoroRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<PomodoroSessionDto> EndAsync(Guid userId, EndPomodoroRequestDto request, CancellationToken cancellationToken = default)
    {
        var session = await _pomodoroRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException($"Pomodoro session '{request.SessionId}' was not found.");

        if (session.UserId != userId)
            throw new UnauthorizedException("You do not have access to this session.");

        if (session.EndTime is not null)
            throw new ValidationException("This Pomodoro session has already ended.");

        session.EndTime = DateTime.UtcNow;
        await _pomodoroRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualMinutes = (session.EndTime.Value - session.StartTime).TotalMinutes;
        var minimumMinutes = Math.Max(1, session.DurationMinutes * 0.8);
        if (actualMinutes >= minimumMinutes)
        {
            await _missionService.IncrementByTriggerKeyAsync(userId, "pomodoro_complete", 1, cancellationToken);
            var studyMinutes = Math.Max(1, (int)Math.Round(actualMinutes));
            await _missionService.IncrementByTriggerKeyAsync(userId, "study_minutes", studyMinutes, cancellationToken);
            
            // Trigger long focus session check
            await _missionService.IncrementByTriggerKeyAsync(userId, "long_focus_session", studyMinutes, cancellationToken);

            // Trigger streak day check
            var todayStats = await _pomodoroRepository.GetStatsAsync(userId, DateTime.UtcNow.Date, DateTime.UtcNow, cancellationToken);
            var minutesToday = todayStats.totalMinutes;
            var minutesBeforeThisSession = minutesToday - studyMinutes;
            if (minutesToday >= 25 && minutesBeforeThisSession < 25)
            {
                await _missionService.IncrementByTriggerKeyAsync(userId, "study_streak_days", 1, cancellationToken);
            }
        }

        return session.ToDto();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Cancelling hard-deletes the session record so it does not appear in history
    /// and does not trigger any mission/stats updates.
    /// </remarks>
    public async Task CancelAsync(Guid userId, CancelPomodoroRequestDto request, CancellationToken cancellationToken = default)
    {
        var session = await _pomodoroRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException($"Pomodoro session '{request.SessionId}' was not found.");

        if (session.UserId != userId)
            throw new UnauthorizedException("You do not have access to this session.");

        if (session.EndTime is not null)
            throw new ValidationException("Cannot cancel a session that has already ended. Use /end to complete it.");

        await _pomodoroRepository.DeleteAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<PomodoroSessionDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var total = await _pomodoroRepository.CountHistoryAsync(userId, cancellationToken);
        var sessions = await _pomodoroRepository.GetHistoryAsync(userId, page, pageSize, cancellationToken);

        return new PagedResult<PomodoroSessionDto>
        {
            Items = sessions.Select(s => s.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PomodoroStatsDto> GetStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var (s7, m7) = await _pomodoroRepository.GetStatsAsync(userId, now.AddDays(-7), now, cancellationToken);
        var (s30, m30) = await _pomodoroRepository.GetStatsAsync(userId, now.AddDays(-30), now, cancellationToken);
        return new PomodoroStatsDto(s7, m7, s30, m30);
    }
}
