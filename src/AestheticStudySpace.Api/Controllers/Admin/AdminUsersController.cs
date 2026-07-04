using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Admin;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IRetentionEmailService _retentionEmailService;

    public AdminUsersController(IAdminService adminService, IRetentionEmailService retentionEmailService)
    {
        _adminService = adminService;
        _retentionEmailService = retentionEmailService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUserAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.Ok(result));
    }

    [HttpPut("{id:guid}/ban")]
    public async Task<ActionResult<ApiResponse<object>>> Ban([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _adminService.BanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { banned = true }));
    }

    [HttpPut("{id:guid}/unban")]
    public async Task<ActionResult<ApiResponse<object>>> Unban([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _adminService.UnbanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { banned = false }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _adminService.DeleteUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }

    /// <summary>
    /// Cập nhật AccountTier của user (Free ↔ Premium).
    /// Body: { "tier": "Free" } hoặc { "tier": "Premium" }
    /// </summary>
    [HttpPut("{id:guid}/tier")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateTier(
        [FromRoute] Guid id,
        [FromBody] UpdateUserTierRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.UpdateUserTierAsync(id, request.Tier, cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.Ok(result));
    }

    /// <summary>
    /// Kích hoạt gửi email nhắc nhở cho tất cả các user không hoạt động trên 7 ngày ngay lập tức.
    /// </summary>
    [HttpPost("trigger-retention-emails")]
    public async Task<ActionResult<ApiResponse<object>>> TriggerRetentionEmails(CancellationToken cancellationToken = default)
    {
        var sentCount = await _retentionEmailService.SendRetentionEmailsAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { sentCount, message = $"Successfully triggered retention emails. Sent count: {sentCount}" }));
    }

    /// <summary>
    /// Kích hoạt gửi email nhắc nhở test cho một người dùng cụ thể ngay lập tức.
    /// </summary>
    [HttpPost("trigger-retention-email/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> TriggerRetentionEmailForUser([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        var success = await _retentionEmailService.SendRetentionEmailToUserAsync(userId, cancellationToken);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }
        return Ok(ApiResponse<object>.Ok(new { success = true, message = "Successfully sent manual test email." }));
    }
}

