using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Store;

namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IUserThemeService
{
    /// <summary>User submits a new theme for admin review.</summary>
    Task<UserThemeSubmissionDto> SubmitThemeAsync(Guid userId, SubmitThemeRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Returns all themes the user has submitted (any status).</summary>
    Task<PagedResult<UserThemeSubmissionDto>> GetMySubmissionsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Get details of a single submitted theme owned by the user.</summary>
    Task<UserThemeSubmissionDto> GetMySubmissionByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraw a submission (only allowed when status is PendingReview or Rejected).
    /// Soft-deletes the item.
    /// </summary>
    Task WithdrawSubmissionAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
