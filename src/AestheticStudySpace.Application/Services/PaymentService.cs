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

    public PaymentService(
        IPaymentTransactionRepository paymentTxRepo,
        IUserRepository userRepo,
        IPaymentFulfillmentService fulfillment,
        IOptions<VnPaySettings> vnPay,
        IUnitOfWork unitOfWork)
    {
        _paymentTxRepo = paymentTxRepo;
        _userRepo = userRepo;
        _fulfillment = fulfillment;
        _unitOfWork = unitOfWork;
        _vnPay = vnPay.Value;
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

        var cleanDescription = RemoveDiacritics(request.Description ?? "Payment");

        var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) || request.ReturnUrl.Equals("string", StringComparison.OrdinalIgnoreCase)
            ? "https://aesthetic-study-space-api.onrender.com/api/payment/vnpay/callback"
            : request.ReturnUrl.Trim();

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _vnPay.TmnCode,
            ["vnp_Amount"] = (request.AmountVnd * 100).ToString(),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = txCode,
            ["vnp_OrderInfo"] = cleanDescription,
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            ["vnp_IpAddr"] = "0.0.0.0"
        };

        var queryString = BuildQueryString(vnpParams);
        var signData = queryString;
        var secureHash = HmacSha512Hex(_vnPay.HashSecret, signData);

        var paymentUrl = $"{_vnPay.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        return new VnPayCreateResponseDto(txCode, paymentUrl);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Payment";
        
        // Chuyển về dạng không dấu tiếng Việt chuẩn
        string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
            "í", "ì", "ỉ", "ĩ", "ị",
            "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ",
            "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự",
            "ý", "ỳ", "ỷ", "ỹ", "ỵ",};
        string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
            "i", "i", "i", "i", "i",
            "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
            "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u",
            "y", "y", "y", "y", "y",};
        for (int i = 0; i < arr1.Length; i++)
        {
            text = text.Replace(arr1[i], arr2[i]);
            text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
        }
        
        // Loại bỏ các ký tự đặc biệt chỉ giữ lại chữ, số và khoảng trắng
        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
            {
                sb.Append(c);
            }
        }
        
        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? "Payment" : result;
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

    private static string BuildQueryString(SortedDictionary<string, string> values) =>
        string.Join("&", values
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

    private static string HmacSha512Hex(string secret, string data)
    {
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }

    private static PaymentPurpose ParsePurpose(string purpose)
    {
        if (Enum.TryParse<PaymentPurpose>(purpose, true, out var result))
            return result;
        throw new ValidationException("Invalid payment purpose.");
    }
}

