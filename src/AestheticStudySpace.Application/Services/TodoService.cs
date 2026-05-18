using AestheticStudySpace.Application.DTOs.Todos;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;

namespace AestheticStudySpace.Application.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TodoService(ITodoRepository todoRepository, IUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TodoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var todos = await _todoRepository.GetByUserIdAsync(userId, cancellationToken);
        return todos.Select(t => t.ToDto()).ToList();
    }

    public async Task<TodoDto> CreateAsync(Guid userId, CreateTodoRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Todo content is required.");

        var todo = new Todo
        {
            UserId = userId,
            Content = request.Content.Trim()
        };

        await _todoRepository.AddAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return todo.ToDto();
    }

    public async Task<TodoDto> UpdateAsync(Guid userId, Guid id, UpdateTodoRequestDto request, CancellationToken cancellationToken = default)
    {
        var todo = await _todoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Todo '{id}' was not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedException("You do not have access to this todo.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Todo content is required.");

        todo.Content = request.Content.Trim();
        todo.IsCompleted = request.IsCompleted;

        await _todoRepository.UpdateAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return todo.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var todo = await _todoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Todo '{id}' was not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedException("You do not have access to this todo.");

        await _todoRepository.DeleteAsync(todo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
