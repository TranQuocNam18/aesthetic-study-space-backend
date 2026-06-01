using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Coins;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/coins")]
[Authorize]
public class CoinsController : ControllerBase
{
    private readonly ICoinService _coinService;

    public CoinsController(ICoinService coinService) => _coinService = coinService;

    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponse<CoinBalanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CoinBalanceDto>>> GetBalance(CancellationToken cancellationToken = default)
    {
        var balance = await _coinService.GetBalanceAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<CoinBalanceDto>.Ok(balance));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CoinTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CoinTransactionDto>>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] CoinTransactionType? type = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _coinService.GetTransactionsAsync(User.GetUserId(), page, pageSize, type, cancellationToken);
        return Ok(ApiResponse<PagedResult<CoinTransactionDto>>.Ok(result));
    }
}
