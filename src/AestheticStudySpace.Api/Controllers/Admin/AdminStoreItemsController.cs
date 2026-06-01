using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/store/items")]
[Authorize(Roles = "Admin")]
public class AdminStoreItemsController : ControllerBase
{
    private readonly IAdminStoreService _adminStoreService;

    public AdminStoreItemsController(IAdminStoreService adminStoreService) => _adminStoreService = adminStoreService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStoreItemDto>>>> GetAll(
        [FromQuery] StoreCategory? category,
        [FromQuery] bool includeInactive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminStoreService.GetItemsAsync(category, includeInactive, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminStoreItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> Create(
        [FromBody] CreateStoreItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Store item created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> Update(
        Guid id,
        [FromBody] UpdateStoreItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Store item updated."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _adminStoreService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Store item deactivated."));
    }
}
