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

    // ── CRUD ──────────────────────────────────────────────────────────────────

    /// <summary>List all store items (admin view, includes inactive + creator info).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStoreItemDto>>>> GetAll(
        [FromQuery] StoreCategory? category,
        [FromQuery] StoreThemeSource? themeSource,
        [FromQuery] bool includeInactive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminStoreService.GetItemsAsync(category, themeSource, includeInactive, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminStoreItemDto>>.Ok(result));
    }

    /// <summary>Get a single store item by ID (admin view).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item));
    }

    /// <summary>Create a new store item directly (Admin-created, goes live immediately).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> Create(
        [FromBody] CreateStoreItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Store item created."));
    }

    /// <summary>Update an existing store item.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> Update(
        Guid id,
        [FromBody] UpdateStoreItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Store item updated."));
    }

    /// <summary>Partially update an existing store item.</summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> Patch(
        Guid id,
        [FromBody] PatchStoreItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.PatchAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Store item updated."));
    }

    /// <summary>Soft-delete (deactivate) a store item.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _adminStoreService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Store item deactivated."));
    }



    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStoreItemDto>>>> GetPendingSubmissions(
        [FromQuery] StoreCategory? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminStoreService.GetPendingSubmissionsAsync(category, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminStoreItemDto>>.Ok(result));
    }

    /// <summary>
    /// Approve a user-submitted Theme.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> ApproveTheme(
        Guid id,
        [FromBody] ApproveThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.ApprovePendingThemeAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Theme approved and is now visible in the store."));
    }

    /// <summary>
    /// Reject a user-submitted Theme combo.
    /// A rejection note explaining the reason is required.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> RejectTheme(
        Guid id,
        [FromBody] RejectThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.RejectPendingThemeAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Theme rejected."));
    }

    /// <summary>
    /// Approve a user-submitted standalone component (Sticker / Background / Effect / AmbientSound).
    /// Admin can optionally adjust pricing before approving.
    /// </summary>
    [HttpPost("{id:guid}/approve-component")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> ApproveComponent(
        Guid id,
        [FromBody] ApproveComponentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.ApprovePendingComponentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Component approved and is now visible in the store."));
    }

    /// <summary>
    /// Reject a user-submitted standalone component (Sticker / Background / Effect / AmbientSound).
    /// Sets status to Rejected, item stays hidden. A rejection note is required.
    /// </summary>
    [HttpPost("{id:guid}/reject-component")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> RejectComponent(
        Guid id,
        [FromBody] RejectThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.RejectPendingComponentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Component rejected."));
    }
}
