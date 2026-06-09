using System.Text;
using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Payments;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly string _frontendBaseUrl;

    public PaymentController(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _frontendBaseUrl = (configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
    }

    [HttpPost("vnpay/create")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<VnPayCreateResponseDto>>> CreateVnPay([FromBody] CreateVnPayPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _paymentService.CreateVnPayAsync(userId, request, cancellationToken);
        return ApiResponse<VnPayCreateResponseDto>.Ok(result);
    }

    [HttpGet("vnpay/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayCallback(CancellationToken cancellationToken = default)
    {
        try
        {
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            await _paymentService.HandleVnPayCallbackAsync(dict, cancellationToken);
            return Redirect($"{_frontendBaseUrl}/payment/result?status=success");
        }
        catch
        {
            return Redirect($"{_frontendBaseUrl}/payment/result?status=failed");
        }
    }

}

