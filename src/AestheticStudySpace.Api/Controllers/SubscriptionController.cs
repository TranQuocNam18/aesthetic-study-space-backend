using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Payments;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/subscription")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService) => _subscriptionService = subscriptionService;

    [HttpPost("upgrade")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Upgrade([FromBody] SubscriptionUpgradeRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _subscriptionService.UpgradeAsync(userId, request, cancellationToken);
        return ApiResponse<object>.Ok(result);
    }
}

