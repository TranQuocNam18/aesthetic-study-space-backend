using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminPaymentController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminPaymentTransactionDto>>>> GetPayments(
        [FromQuery] string? search,
        [FromQuery] PaymentProvider? provider,
        [FromQuery] PaymentStatus? status,
        [FromQuery] PaymentPurpose? purpose,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetPaymentsAsync(search, provider, status, purpose, fromDate, toDate, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminPaymentTransactionDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminPaymentTransactionDto>>> GetPaymentById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetPaymentByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminPaymentTransactionDto>.Ok(result));
    }

    [HttpPost("{id:guid}/fulfill")]
    public async Task<ActionResult<ApiResponse<object>>> FulfillPayment(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await _adminService.ManualFulfillPaymentAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { fulfilled = true }));
    }
}
