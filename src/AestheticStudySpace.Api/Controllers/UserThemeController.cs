using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

/// <summary>
/// Endpoints for authenticated users to submit and manage their own theme submissions.
/// </summary>
[ApiController]
[Route("api/me/themes")]
[Authorize]
public class UserThemeController : ControllerBase
{
    private readonly IUserThemeService _userThemeService;

    public UserThemeController(IUserThemeService userThemeService) => _userThemeService = userThemeService;

    /// <summary>
    /// Submit a new theme for admin review.
    /// The theme will be visible in the store only after an admin approves it.
    /// AssetUrl should be a Cloudinary URL already uploaded by the client.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserThemeSubmissionDto>>> SubmitTheme(
        [FromBody] SubmitThemeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userThemeService.SubmitThemeAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<UserThemeSubmissionDto>.Ok(result, "Theme submitted and is pending admin review."));
    }

    /// <summary>
    /// List all themes submitted by the current user (any status: PendingReview, Approved, Rejected).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserThemeSubmissionDto>>>> GetMySubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _userThemeService.GetMySubmissionsAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserThemeSubmissionDto>>.Ok(result));
    }

    /// <summary>
    /// Get details of a specific submitted theme by ID.
    /// Only accessible by the user who submitted it.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserThemeSubmissionDto>>> GetMySubmission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userThemeService.GetMySubmissionByIdAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<UserThemeSubmissionDto>.Ok(result));
    }

    /// <summary>
    /// Withdraw a submitted theme. Only allowed if the theme is still pending review or was rejected.
    /// An approved (live) theme cannot be withdrawn this way.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> WithdrawSubmission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userThemeService.WithdrawSubmissionAsync(User.GetUserId(), id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Theme submission withdrawn."));
    }
}
