namespace AestheticStudySpace.Application.DTOs.Todos;

public record TodoDto(Guid Id, string Content, bool IsCompleted, DateTime CreatedAt);

public record CreateTodoRequestDto(string Content);

public record UpdateTodoRequestDto(string Content, bool IsCompleted);
