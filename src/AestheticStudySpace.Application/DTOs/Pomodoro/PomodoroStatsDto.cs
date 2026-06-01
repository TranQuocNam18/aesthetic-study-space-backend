namespace AestheticStudySpace.Application.DTOs.Pomodoro;

public record PomodoroStatsDto(
    int SessionsLast7Days,
    int TotalMinutesLast7Days,
    int SessionsLast30Days,
    int TotalMinutesLast30Days);

