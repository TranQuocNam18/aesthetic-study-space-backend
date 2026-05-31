using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AestheticStudySpace.Application.DTOs.Payments;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using AestheticStudySpace.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentTransactionRepository _paymentTxRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPaymentFulfillmentService _fulfillment;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VnPaySettings _vnPay;
    private readonly SePaySettings _sePay;

    public PaymentService(
        IPaymentTransactionRepository paymentTxRepo,
        IUserRepository userRepo,
        IPaymentFulfillmentService fulfillment,
        IOptions<VnPaySettings> vnPay,
        IOptions<SePaySettings> sePay,
        IUnitOfWork unitOfWork)
    {
        _paymentTxRepo = paymentTxRepo;
        _userRepo = userRepo;
        _fulfillment = fulfillment;
        _unitOfWork = unitOfWork;
        _vnPay = vnPay.Value;
        _sePay = sePay.Value;
    }

    public async Task<VnPayCreateResponseDto> CreateVnPayAsync(Guid userId, CreateVnPayPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.AmountVnd <= 0) throw new ValidationException("AmountVnd must be positive.");
        if (string.IsNullOrWhiteSpace(_vnPay.TmnCode) || string.IsNullOrWhiteSpace(_vnPay.HashSecret))
            throw new InvalidOperationException("VNPay settings are not configured.");

        _ = await _userRepo.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");

        var txCode = $"VNP{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
        var purpose = ParsePurpose(request.Purpose);
        var tx = new PaymentTransaction
        {
            UserId = userId,
            Provider = PaymentProvider.VNPay,
            Status = PaymentStatus.Pending,
            Purpose = purpose,
            TransactionCode = txCode,
            Amount = request.AmountVnd,
            Currency = "VND",
            ProviderPayloadJson = JsonSerializer.Serialize(new { request.Description }),
            MetadataJson = JsonSerializer.Serialize(new
            {
                storeItemId = request.StoreItemId?.ToString(),
                coinsAmount = request.CoinsAmount?.ToString()
            })
        };

        await _paymentTxRepo.AddAsync(tx, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _vnPay.TmnCode,
            ["vnp_Amount"] = (request.AmountVnd * 100).ToString(),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = txCode,
            ["vnp_OrderInfo"] = (request.Description ?? "Payment").Trim(),
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = request.ReturnUrl.Trim(),
            ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            ["vnp_IpAddr"] = "0.0.0.0"
        };

        var queryString = BuildQueryString(vnpParams);
        var signData = queryString;
        var secureHash = HmacSha512Hex(_vnPay.HashSecret, signData);

        var paymentUrl = $"{_vnPay.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        return new VnPayCreateResponseDto(txCode, paymentUrl);
    }

    public async Task HandleVnPayCallbackAsync(IReadOnlyDictionary<string, string> query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_vnPay.HashSecret))
            throw new InvalidOperationException("VNPay settings are not configured.");

        var dict = query
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        if (!dict.TryGetValue("vnp_TxnRef", out var txRef) || string.IsNullOrWhiteSpace(txRef))
            throw new ValidationException("Missing vnp_TxnRef.");

        dict.TryGetValue("vnp_SecureHash", out var secureHash);
        dict.Remove("vnp_SecureHash");
        dict.Remove("vnp_SecureHashType");

        var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
        var signData = BuildQueryString(sorted);
        var expected = HmacSha512Hex(_vnPay.HashSecret, signData);

        if (!string.Equals(expected, secureHash, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedException("Invalid VNPay signature.");

        var tx = await _paymentTxRepo.GetByTransactionCodeAsync(txRef, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        query.TryGetValue("vnp_ResponseCode", out var responseCode);
        if (responseCode == "00")
        {
            tx.Status = PaymentStatus.Succeeded;
            tx.SucceededAt = DateTime.UtcNow;
        }
        else
        {
            tx.Status = PaymentStatus.Failed;
            tx.FailedAt = DateTime.UtcNow;
        }

        tx.ProviderPayloadJson = JsonSerializer.Serialize(dict);
        await _paymentTxRepo.UpdateAsync(tx, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fulfillment.FulfillIfNeededAsync(tx, cancellationToken);
    }

    public async Task<SePayCreateResponseDto> CreateSePayAsync(Guid userId, CreateSePayPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.AmountVnd <= 0) throw new ValidationException("AmountVnd must be positive.");
        _ = await _userRepo.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException("User not found.");

        var txCode = $"SEP{DateTime.UtcNow:yyyyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
        var purpose = ParsePurpose(request.Purpose);
        var tx = new PaymentTransaction
        {
            UserId = userId,
            Provider = PaymentProvider.SePay,
            Status = PaymentStatus.Pending,
            Purpose = purpose,
            TransactionCode = txCode,
            Amount = request.AmountVnd,
            Currency = "VND",
            ProviderPayloadJson = JsonSerializer.Serialize(new { request.Description }),
            MetadataJson = JsonSerializer.Serialize(new
            {
                storeItemId = request.StoreItemId?.ToString(),
                coinsAmount = request.CoinsAmount?.ToString()
            })
        };

        await _paymentTxRepo.AddAsync(tx, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // SePay payment link creation depends on merchant portal; we still create the transaction + code.
        return new SePayCreateResponseDto(txCode);
    }

    public async Task HandleSePayWebhookAsync(string rawBody, string? signature, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_sePay.WebhookSecret))
            throw new InvalidOperationException("SePay settings are not configured.");

        if (string.IsNullOrWhiteSpace(signature))
            throw new UnauthorizedException("Missing SePay signature.");

        var expected = HmacSha256Hex(_sePay.WebhookSecret, rawBody);
        if (!FixedTimeEquals(expected, signature.Trim()))
            throw new UnauthorizedException("Invalid SePay signature.");

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        var txCode = root.TryGetProperty("transactionCode", out var codeEl) ? codeEl.GetString() : null;
        var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(txCode))
            throw new ValidationException("Missing transactionCode.");

        var tx = await _paymentTxRepo.GetByTransactionCodeAsync(txCode, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        if (string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            tx.Status = PaymentStatus.Succeeded;
            tx.SucceededAt = DateTime.UtcNow;
        }
        else if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            tx.Status = PaymentStatus.Failed;
            tx.FailedAt = DateTime.UtcNow;
        }

        tx.ProviderPayloadJson = rawBody;
        await _paymentTxRepo.UpdateAsync(tx, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fulfillment.FulfillIfNeededAsync(tx, cancellationToken);
    }

    private static string BuildQueryString(SortedDictionary<string, string> values)
    {
        var query = new StringBuilder();
        foreach (var kv in values)
        {
            if (string.IsNullOrWhiteSpace(kv.Value)) continue;

            if (query.Length > 0)
            {
                query.Append('&');
            }

            // VNPay yêu cầu mã hóa cả Key và Value theo chuẩn, khoảng trắng biến thành %20 thay vì +
            var encodedKey = System.Net.WebUtility.UrlEncode(kv.Key);
            var encodedVal = System.Net.WebUtility.UrlEncode(kv.Value);
            
            // WebUtility.UrlEncode biến khoảng trắng thành "+", ta cần đổi lại thành "%20" chuẩn VNPay
            encodedKey = encodedKey.Replace("+", "%20");
            encodedVal = encodedVal.Replace("+", "%20");

            query.Append(encodedKey).Append('=').Append(encodedVal);
        }
        return query.ToString();
    }

    private static string HmacSha512Hex(string secret, string data)
    {
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }

    private static string HmacSha256Hex(string secret, string data)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        try
        {
            var ba = Convert.FromHexString(a);
            var bb = Convert.FromHexString(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
        catch
        {
            return false;
        }
    }

    private static PaymentPurpose ParsePurpose(string purpose)
    {
        if (Enum.TryParse<PaymentPurpose>(purpose, true, out var result))
            return result;
        throw new ValidationException("Invalid payment purpose.");
    }
}

