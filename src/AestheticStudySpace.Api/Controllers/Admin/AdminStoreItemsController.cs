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





    // ── Creator Buyout Transaction & Pricing Pool Workflow ─────────────────────

    [HttpGet("pending-transactions")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStoreItemDto>>>> GetPendingTransactions(
        [FromQuery] StoreCategory? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminStoreService.GetPendingTransactionsAsync(category, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminStoreItemDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/approve-transaction")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> ApproveTransaction(
        Guid id,
        [FromBody] AdminApproveTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.ApproveTransactionAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Transaction approved. Item moved to pricing pool."));
    }

    [HttpPost("{id:guid}/reject-transaction")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> RejectTransaction(
        Guid id,
        [FromBody] RejectThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.RejectTransactionAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Transaction rejected."));
    }

    [HttpGet("purchased-pending-pricing")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStoreItemDto>>>> GetPurchasedPendingPricing(
        [FromQuery] StoreCategory? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminStoreService.GetPurchasedPendingPricingAsync(category, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminStoreItemDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/price-publish")]
    public async Task<ActionResult<ApiResponse<AdminStoreItemDto>>> PriceAndPublish(
        Guid id,
        [FromBody] AdminPriceAndPublishDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await _adminStoreService.PriceAndPublishAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminStoreItemDto>.Ok(item, "Item has been priced and published successfully to store."));
    }
}
