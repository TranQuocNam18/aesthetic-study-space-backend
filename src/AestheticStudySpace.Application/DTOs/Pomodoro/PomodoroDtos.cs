namespace AestheticStudySpace.Application.DTOs.Pomodoro;

public record StartPomodoroRequestDto(int DurationMinutes);

public record PomodoroSessionDto(
    Guid Id,
    DateTime StartTime,
    DateTime? EndTime,
    int DurationMinutes,
    bool IsActive);

public record EndPomodoroRequestDto(Guid SessionId);
