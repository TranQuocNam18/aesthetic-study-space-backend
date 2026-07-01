namespace AestheticStudySpace.Application.DTOs.Workspace;

public record WelcomeBackDto(
    string Message,
    int YesterdayFocusMinutes,
    int YesterdayCompletedTasksCount
);
