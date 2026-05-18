using AestheticStudySpace.Application.DTOs.Todos;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface ITodoService
{
    Task<IReadOnlyList<TodoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(Guid userId, CreateTodoRequestDto request, CancellationToken cancellationToken = default);
    Task<TodoDto> UpdateAsync(Guid userId, Guid id, UpdateTodoRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
