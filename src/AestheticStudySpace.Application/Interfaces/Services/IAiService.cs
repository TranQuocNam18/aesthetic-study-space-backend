namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IAiService
{
    Task<string> GenerateWelcomeMessageAsync(string username, int completedTasksCount, int totalFocusMinutes, CancellationToken cancellationToken = default);
}
