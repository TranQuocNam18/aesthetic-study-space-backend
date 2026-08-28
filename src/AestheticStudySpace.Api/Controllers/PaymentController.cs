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

    // ── VNPay ────────────────────────────────────────────────────────────────

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

    // ── PayOS ────────────────────────────────────────────────────────────────

    /// <summary>Tạo link thanh toán PayOS — yêu cầu đăng nhập.</summary>
    [HttpPost("payos/create")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PayOsCreateResponseDto>>> CreatePayOs([FromBody] CreatePayOsPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var result = await _paymentService.CreatePayOsAsync(userId, request, cancellationToken);
        return ApiResponse<PayOsCreateResponseDto>.Ok(result);
    }

    /// <summary>Webhook PayOS gọi về sau khi giao dịch hoàn tất.</summary>
    [HttpPost("payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook(CancellationToken cancellationToken = default)
    {
        using var reader = new System.IO.StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync(cancellationToken);
        try
        {
            await _paymentService.HandlePayOsWebhookAsync(body, cancellationToken);
            return Ok(new { success = true });
        }
        catch
        {
            // Trả 200 để PayOS không retry; lỗi đã được log
            return Ok(new { success = false });
        }
    }

    /// <summary>Trang redirect khi người dùng thanh toán thành công trên PayOS.</summary>
    [HttpGet("payos/return")]
    [AllowAnonymous]
    public IActionResult PayOsReturn()
        => Redirect($"{_frontendBaseUrl}/payment/result?status=success&provider=payos");

    /// <summary>Trang redirect khi người dùng huỷ thanh toán trên PayOS.</summary>
    [HttpGet("payos/cancel")]
    [AllowAnonymous]
    public IActionResult PayOsCancel()
        => Redirect($"{_frontendBaseUrl}/payment/result?status=cancelled&provider=payos");
}


