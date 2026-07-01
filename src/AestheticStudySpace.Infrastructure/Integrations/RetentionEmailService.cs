using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class RetentionEmailService : IRetentionEmailService
{
    private const int InactiveDaysThreshold = 7;
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RetentionEmailService> _logger;

    public RetentionEmailService(
        AppDbContext context,
        IEmailSender emailSender,
        ILogger<RetentionEmailService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<int> SendRetentionEmailsAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-InactiveDaysThreshold);

        // Find users who:
        // 1. Are not banned.
        // 2. Last activity (LastLoginAt, fallback to CreatedAt if never logged in) is at least 7 days ago.
        // 3. Haven't received a retention email since their last activity.
        var inactiveUsers = await _context.Users
            .Where(u => !u.IsBanned)
            .Where(u => (u.LastLoginAt ?? u.CreatedAt) <= cutoffDate)
            .Where(u => u.LastRetentionEmailSentAt == null || u.LastRetentionEmailSentAt < (u.LastLoginAt ?? u.CreatedAt))
            .Take(20) // Giới hạn tối đa 20 email mỗi lần chạy để bảo vệ tài khoản Resend Free tier (tối đa 100/ngày)
            .ToListAsync(cancellationToken);

        if (!inactiveUsers.Any())
        {
            _logger.LogInformation("No inactive users found for retention emails.");
            return 0;
        }

        _logger.LogInformation("Found {Count} inactive users. Sending retention emails...", inactiveUsers.Count);
        int successfullySent = 0;

        foreach (var user in inactiveUsers)
        {
            try
            {
                var emailBody = GetRetentionEmailHtml(user.Username);
                
                await _emailSender.SendAsync(
                    user.Email,
                    "Góc học tập Aesthetic Study Space đang đợi bạn! 🎧📚",
                    emailBody,
                    cancellationToken
                );

                user.LastRetentionEmailSentAt = DateTime.UtcNow;
                successfullySent++;
                _logger.LogInformation("Successfully sent retention email to {Username} ({Email})", user.Username, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send retention email to user {UserId} ({Email})", user.Id, user.Email);
            }
        }

        if (successfullySent > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return successfullySent;
    }

    private string GetRetentionEmailHtml(string username)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Chúng tôi nhớ bạn!</title>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #f7f9fc;
                    margin: 0;
                    padding: 0;
                    color: #333333;
                }}
                .container {{
                    max-width: 600px;
                    margin: 40px auto;
                    background: #ffffff;
                    border-radius: 12px;
                    box-shadow: 0 4px 15px rgba(0,0,0,0.05);
                    overflow: hidden;
                }}
                .header {{
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    padding: 40px 20px;
                    text-align: center;
                    color: #ffffff;
                }}
                .header h1 {{
                    margin: 0;
                    font-size: 26px;
                    font-weight: 600;
                }}
                .content {{
                    padding: 30px;
                    line-height: 1.6;
                }}
                .content p {{
                    font-size: 16px;
                    margin-bottom: 20px;
                }}
                .btn-container {{
                    text-align: center;
                    margin: 30px 0;
                }}
                .btn {{
                    background: #764ba2;
                    color: #ffffff !important;
                    text-decoration: none;
                    padding: 12px 30px;
                    font-weight: bold;
                    border-radius: 30px;
                    display: inline-block;
                    box-shadow: 0 4px 6px rgba(118, 75, 162, 0.2);
                }}
                .footer {{
                    background-color: #f1f3f7;
                    text-align: center;
                    padding: 20px;
                    font-size: 13px;
                    color: #777777;
                    border-top: 1px solid #e9ecef;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Aesthetic Study Space</h1>
                </div>
                <div class='content'>
                    <p>Chào <strong>{username}</strong>,</p>
                    <p>Đã 7 ngày rồi chúng tôi không thấy bạn ghé thăm không gian học tập của mình. Những bản nhạc lofi êm dịu, âm thanh mưa rơi và góc làm việc ảo yêu thích của bạn vẫn đang chờ bạn đấy!</p>
                    <p>Hãy dành ra một khoảng thời gian nhỏ hôm nay để ngồi vào bàn học, bật Pomodoro và hoàn thành những mục tiêu còn dang dở nhé.</p>
                    <div class='btn-container'>
                        <a href='https://www.aestheticspace.live' class='btn'>Quay lại học tập ngay</a>
                    </div>
                    <p>Chúc bạn một ngày học tập và làm việc thật hiệu quả!</p>
                    <p>Thân mến,<br>Đội ngũ Aesthetic Study Space</p>
                </div>
                <div class='footer'>
                    <p>Bạn nhận được email này vì đã đăng ký tài khoản trên Aesthetic Study Space.</p>
                    <p>&copy; {DateTime.UtcNow.Year} Aesthetic Study Space. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";
    }
}
