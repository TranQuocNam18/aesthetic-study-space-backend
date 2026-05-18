using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Todos;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService) => _todoService = todoService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TodoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TodoDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var todos = await _todoService.GetAllAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TodoDto>>.Ok(todos));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TodoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Create([FromBody] CreateTodoRequestDto request, CancellationToken cancellationToken)
    {
        var todo = await _todoService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<TodoDto>.Ok(todo, "Todo created."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TodoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Update(Guid id, [FromBody] UpdateTodoRequestDto request, CancellationToken cancellationToken)
    {
        var todo = await _todoService.UpdateAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(ApiResponse<TodoDto>.Ok(todo, "Todo updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _todoService.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Todo deleted."));
    }
}
