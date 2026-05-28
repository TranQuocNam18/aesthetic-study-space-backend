using System.Text;
using AestheticStudySpace.Api.Extensions;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.DTOs.Payments;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

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
        var dict = Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        await _paymentService.HandleVnPayCallbackAsync(dict, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("sepay/create")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SePayCreateResponseDto>>> CreateSePay([FromBody] CreateSePayPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _paymentService.CreateSePayAsync(userId, request, cancellationToken);
        return ApiResponse<SePayCreateResponseDto>.Ok(result);
    }

    [HttpPost("sepay/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> SePayWebhook(CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var raw = await reader.ReadToEndAsync(cancellationToken);
        var sig = Request.Headers["X-SePay-Signature"].ToString();

        await _paymentService.HandleSePayWebhookAsync(raw, sig, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }
}

