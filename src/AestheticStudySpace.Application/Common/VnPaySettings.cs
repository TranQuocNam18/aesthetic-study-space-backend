namespace AestheticStudySpace.Application.Common;

public class VnPaySettings
{
    public const string SectionName = "VNPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
}

