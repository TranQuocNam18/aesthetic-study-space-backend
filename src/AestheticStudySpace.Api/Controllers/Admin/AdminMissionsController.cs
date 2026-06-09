using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Missions;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/missions")]
[Authorize(Roles = "Admin")]
public class AdminMissionsController : ControllerBase
{
    private readonly IAdminMissionService _adminMissionService;

    public AdminMissionsController(IAdminMissionService adminMissionService) => _adminMissionService = adminMissionService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminMissionDto>>>> GetAll(
        [FromQuery] bool includeInactive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminMissionService.GetMissionsAsync(includeInactive, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminMissionDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminMissionDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _adminMissionService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminMissionDto>.Ok(mission));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminMissionDto>>> Create(
        [FromBody] CreateMissionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var mission = await _adminMissionService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminMissionDto>.Ok(mission, "Mission created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminMissionDto>>> Update(
        Guid id,
        [FromBody] UpdateMissionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var mission = await _adminMissionService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminMissionDto>.Ok(mission, "Mission updated."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _adminMissionService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Mission deactivated."));
    }
}
