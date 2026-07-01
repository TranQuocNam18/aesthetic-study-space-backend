using AestheticStudySpace.Application.DTOs.Workspace;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class WelcomeBackService : IWelcomeBackService
{
    private readonly IUserRepository _userRepository;
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ITodoRepository _todoRepository;
    private readonly IAiService _aiService;

    public WelcomeBackService(
        IUserRepository userRepository,
        IPomodoroRepository pomodoroRepository,
        ITodoRepository todoRepository,
        IAiService aiService)
    {
        _userRepository = userRepository;
        _pomodoroRepository = pomodoroRepository;
        _todoRepository = todoRepository;
        _aiService = aiService;
    }

    public async Task<WelcomeBackDto> GetWelcomeBackMessageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User with ID '{userId}' was not found.");

        // Define yesterday range (UTC)
        var startOfYesterday = DateTime.UtcNow.Date.AddDays(-1);
        var endOfYesterday = DateTime.UtcNow.Date;

        // Get Pomodoro stats for yesterday
        var (_, totalMinutes) = await _pomodoroRepository.GetStatsAsync(userId, startOfYesterday, endOfYesterday, cancellationToken);

        // Get completed Todos for yesterday
        var todos = await _todoRepository.GetByUserIdAsync(userId, cancellationToken);
        var completedTasksCount = todos.Count(t => t.IsCompleted && t.UpdatedAt >= startOfYesterday && t.UpdatedAt < endOfYesterday);

        // Generate AI Welcome Message
        var welcomeMessage = await _aiService.GenerateWelcomeMessageAsync(
            user.Username,
            completedTasksCount,
            totalMinutes,
            cancellationToken);

        return new WelcomeBackDto(welcomeMessage, totalMinutes, completedTasksCount);
    }
}
