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
    private readonly IUnitOfWork _unitOfWork;

    public PomodoroService(IPomodoroRepository pomodoroRepository, IUnitOfWork unitOfWork)
    {
        _pomodoroRepository = pomodoroRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PomodoroSessionDto> StartAsync(Guid userId, StartPomodoroRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.DurationMinutes is < 1 or > 120)
            throw new ValidationException("Duration must be between 1 and 120 minutes.");

        var active = await _pomodoroRepository.GetActiveSessionAsync(userId, cancellationToken);
        if (active is not null)
            throw new ValidationException("An active Pomodoro session already exists. End it before starting a new one.");

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
        return session.ToDto();
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
}
