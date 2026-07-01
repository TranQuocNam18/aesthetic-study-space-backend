using AestheticStudySpace.Application.DTOs.Report;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace AestheticStudySpace.Application.Services;

public class ReportService : IReportService
{
    private const string SupportEmail = "aestheticspaceprojectexe201@gmail.com";

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IReportRepository _reportRepository;
    private readonly IMediaStorageService _mediaStorageService;
    private readonly string _backendBaseUrl;

    public ReportService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IReportRepository reportRepository,
        IMediaStorageService mediaStorageService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _reportRepository = reportRepository;
        _mediaStorageService = mediaStorageService;
        _backendBaseUrl = configuration["App:BackendBaseUrl"] ?? "http://localhost:8080";
    }

    public async Task<ReportResponseDto> CreateReportAsync(
        Guid userId,
        CreateReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Content is required.");

        if (request.Title.Length > 256)
            throw new ValidationException("Title must not exceed 256 characters.");

        if (request.Content.Length > 4000)
            throw new ValidationException("Content must not exceed 4000 characters.");

        var normalizedType = request.Type?.Trim() ?? "Feedback";
        if (!normalizedType.Equals("Feedback", StringComparison.OrdinalIgnoreCase) &&
            !normalizedType.Equals("Bug", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Report type must be either 'Feedback' or 'Bug'.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        string? finalAttachmentUrl = request.AttachmentUrl?.Trim();

        // If a base64 image is directly supplied, upload it to Cloudinary first
        if (!string.IsNullOrWhiteSpace(request.AttachmentBase64))
        {
            try
            {
                finalAttachmentUrl = await _mediaStorageService.UploadBase64ImageAsync(
                    request.AttachmentBase64,
                    "reports",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Failed to upload attachment: {ex.Message}");
            }
        }

        var report = new Report
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Type = normalizedType,
            AttachmentUrl = finalAttachmentUrl,
            Status = "Pending"
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email notification to support inbox
        await SendSupportEmailAsync(user, report, cancellationToken);

        return new ReportResponseDto(
            report.Id, 
            report.Title, 
            report.Content, 
            report.Type, 
            report.AttachmentUrl, 
            report.Status, 
            report.CreatedAt);
    }

    private async Task SendSupportEmailAsync(Domain.Entities.User user, Report report, CancellationToken cancellationToken)
    {
        try
        {
            var subject = $"[{report.Type.ToUpper()} #{report.Id.ToString()[..8].ToUpper()}] {report.Title}";
            var htmlBody = BuildEmailBody(user, report, _backendBaseUrl);
            await _emailSender.SendAsync(SupportEmail, subject, htmlBody, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to send support email: {ex.Message}");
        }
    }

    private static string BuildEmailBody(Domain.Entities.User user, Report report, string backendBaseUrl)
    {
        var typeBadgeColor = report.Type.Equals("Bug", StringComparison.OrdinalIgnoreCase) ? "#f8d7da" : "#d1ecf1";
        var typeTextColor = report.Type.Equals("Bug", StringComparison.OrdinalIgnoreCase) ? "#721c24" : "#0c5460";

        var attachmentHtml = string.Empty;
        if (!string.IsNullOrWhiteSpace(report.AttachmentUrl))
        {
            attachmentHtml = $@"
            <div class=""label"">Attachment / Screenshot</div>
            <div class=""value"">
                <a href=""{report.AttachmentUrl}"" target=""_blank"">
                    <img src=""{report.AttachmentUrl}"" alt=""Screenshot"" style=""max-width: 100%; border-radius: 8px; border: 1px solid #ddd; margin-top: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"" />
                </a>
                <br/>
                <a href=""{report.AttachmentUrl}"" target=""_blank"" style=""font-size: 12px; color: #6c63ff; display: inline-block; margin-top: 6px;"">Open full size image</a>
            </div>";
        }

        var fontBaseUrl = backendBaseUrl.TrimEnd('/');
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8""/>
    <style>
        @font-face {{
            font-family: 'HarmonyOS Sans';
            src: url('{fontBaseUrl}/fonts/HarmonyOS_Sans_Regular.ttf') format('truetype');
            font-weight: 400;
            font-style: normal;
        }}
        @font-face {{
            font-family: 'HarmonyOS Sans';
            src: url('{fontBaseUrl}/fonts/HarmonyOS_Sans_Bold.ttf') format('truetype');
            font-weight: 700;
            font-style: normal;
        }}
        body {{ font-family: 'HarmonyOS Sans', 'Segoe UI', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 32px auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #6c63ff, #a78bfa); padding: 28px 32px; color: #fff; }}
        .header h1 {{ margin: 0; font-size: 20px; font-weight: 700; }}
        .header p {{ margin: 4px 0 0; opacity: 0.85; font-size: 13px; }}
        .body {{ padding: 28px 32px; }}
        .label {{ font-size: 11px; font-weight: 600; color: #6c63ff; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 4px; }}
        .value {{ font-size: 14px; color: #1a1a2e; margin-bottom: 20px; word-break: break-word; }}
        .content-box {{ background: #f8f8ff; border-left: 4px solid #6c63ff; border-radius: 6px; padding: 14px 16px; font-size: 14px; color: #333; white-space: pre-wrap; word-break: break-word; }}
        .footer {{ background: #f5f5f5; padding: 16px 32px; font-size: 12px; color: #888; text-align: center; }}
        .badge {{ display: inline-block; border-radius: 4px; padding: 2px 10px; font-size: 12px; font-weight: 600; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>&#x1F6A8; New User {report.Type}</h1>
            <p>Aesthetic Study Space — Support Inbox</p>
        </div>
        <div class=""body"">
            <div class=""label"">Report ID</div>
            <div class=""value""><code>#{report.Id}</code></div>

            <div class=""label"">Report Type</div>
            <div class=""value"">
                <span class=""badge"" style=""background-color: {typeBadgeColor}; color: {typeTextColor};"">{report.Type}</span>
            </div>

            <div class=""label"">Status</div>
            <div class=""value""><span class=""badge"" style=""background-color: #fff3cd; color: #856404;"">{report.Status}</span></div>

            <div class=""label"">Submitted At</div>
            <div class=""value"">{report.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</div>

            <div class=""label"">User Details</div>
            <div class=""value"">
                <strong>{user.Username}</strong> ({user.Email})<br/>
                <span style=""font-size: 12px; color: #888;"">ID: {user.Id}</span>
            </div>

            <div class=""label"">Report Title</div>
            <div class=""value""><strong>{report.Title}</strong></div>

            <div class=""label"">Report Content</div>
            <div class=""content-box"" style=""margin-bottom: 20px;"">{report.Content}</div>

            {attachmentHtml}
        </div>
        <div class=""footer"">
            This notification was generated automatically by Aesthetic Study Space.<br/>
            Please do not reply to this email directly — contact the user at <a href=""mailto:{user.Email}"">{user.Email}</a>.
        </div>
    </div>
</body>
</html>";
    }
}
