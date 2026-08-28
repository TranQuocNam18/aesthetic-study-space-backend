using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class RetentionEmailService : IRetentionEmailService
{
    private const int InactiveDaysThreshold = 7;
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RetentionEmailService> _logger;
    private readonly string _backendBaseUrl;

    public RetentionEmailService(
        AppDbContext context,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<RetentionEmailService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
        _backendBaseUrl = configuration["App:BackendBaseUrl"] ?? "http://localhost:8080";
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
                var emailBody = GetRetentionEmailHtml(user.Username, _backendBaseUrl);
                
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

    public async Task<bool> SendRetentionEmailToUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found for manual retention email test.", userId);
            return false;
        }

        var emailBody = GetRetentionEmailHtml(user.Username, _backendBaseUrl);
        
        await _emailSender.SendAsync(
            user.Email,
            "Góc học tập Aesthetic Study Space đang đợi bạn! 🎧📚 (Test)",
            emailBody,
            cancellationToken
        );

        user.LastRetentionEmailSentAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully sent manual test retention email to {Username} ({Email})", user.Username, user.Email);
        return true;
    }

    private string GetRetentionEmailHtml(string username, string backendBaseUrl)
    {
        var fontBaseUrl = backendBaseUrl.TrimEnd('/');
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Góc học tập đang đợi bạn!</title>
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
                @font-face {{
                    font-family: 'Manrope';
                    src: url('{fontBaseUrl}/fonts/Manrope-VariableFont_wght.ttf') format('truetype');
                    font-weight: 200 800;
                }}
                body {{
                    font-family: 'HarmonyOS Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                    background-color: #0C0F0F;
                    margin: 0;
                    padding: 0;
                    color: #FFFFFF;
                    -webkit-font-smoothing: antialiased;
                }}
                .wrapper {{
                    background-color: #0C0F0F;
                    padding: 40px 20px;
                }}
                .container {{
                    max-width: 550px;
                    margin: 0 auto;
                    background: #161B1B;
                    border-radius: 16px;
                    overflow: hidden;
                    box-shadow: 0 12px 40px rgba(0,0,0,0.5);
                    border: 1px solid #232A2A;
                }}
                .header {{
                    padding: 44px 40px 20px 40px;
                    text-align: center;
                }}
                .logo-container {{
                    text-align: center;
                    margin-bottom: 24px;
                }}
                .logo-text {{
                    font-family: 'Manrope', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    font-size: 28px;
                    font-weight: 500;
                    color: #FFFFFF;
                    letter-spacing: -0.5px;
                    display: inline-block;
                }}
                .header h1 {{
                    margin: 0;
                    font-size: 26px;
                    font-weight: 800;
                    line-height: 1.35;
                    color: #FFFFFF;
                }}
                .content {{
                    padding: 0 40px 44px 40px;
                }}
                .intro-text {{
                    font-size: 15px;
                    line-height: 1.6;
                    color: #A2AAAA;
                    text-align: center;
                    margin-bottom: 32px;
                }}
                .highlight {{
                    color: #00F0C2;
                    font-weight: 600;
                }}
                
                /* Gợi ý tính năng kiểu Spotify Dashboard */
                .feature-box {{
                    background: #1E2424;
                    border-radius: 12px;
                    padding: 20px;
                    margin-bottom: 32px;
                    border: 1px solid #2A3333;
                }}
                .feature-title {{
                    font-size: 12px;
                    text-transform: uppercase;
                    letter-spacing: 1px;
                    color: #00F0C2;
                    margin: 0 0 16px 0;
                    font-weight: 700;
                    text-align: center;
                }}
                .feature-item {{
                    display: flex;
                    align-items: center;
                    padding: 10px 0;
                    border-bottom: 1px solid #2A3333;
                    font-size: 14px;
                    color: #E2E8E8;
                }}
                .feature-item:last-child {{
                    border-bottom: none;
                }}
                .feature-emoji {{
                    margin-right: 14px;
                    font-size: 18px;
                }}
                
                /* Button phong cách Spotify pill-shape */
                .btn-container {{
                    text-align: center;
                    margin: 32px 0 16px 0;
                }}
                .btn {{
                    background: #00F0C2;
                    color: #0C0F0F !important;
                    text-decoration: none;
                    padding: 16px 44px;
                    font-weight: 700;
                    font-size: 15px;
                    border-radius: 50px;
                    display: inline-block;
                    letter-spacing: 0.5px;
                    box-shadow: 0 4px 20px rgba(0, 240, 194, 0.3);
                }}
                
                .footer {{
                    background-color: #0C0F0F;
                    text-align: center;
                    padding: 32px 40px;
                    font-size: 12px;
                    color: #626A6A;
                    line-height: 1.6;
                    border-top: 1px solid #161B1B;
                }}
                .footer a {{
                    color: #A2AAAA;
                    text-decoration: underline;
                }}
            </style>
        </head>
        <body>
            <div class='wrapper'>
                <div class='container'>
                    <div class='header'>
                        <div class='logo-container'>
                            <span class='logo-text'>Aēsthetic Space</span>
                        </div>
                        <h1>Đã 7 ngày rồi, bàn học của bạn đang trống...</h1>
                    </div>
                    <div class='content'>
                        <p class='intro-text'>
                            Chào <strong style='color: #FFFFFF;'>{username}</strong>, không gian ảo của bạn đang hơi yên ắng thiếu đi tiếng lật sách và những bước chân hoàn thành mục tiêu. Hãy quay lại kích hoạt năng lượng tích cực nào!
                        </p>
                        
                        <div class='feature-box'>
                            <p class='feature-title'>Mở lại không gian của bạn</p>
                            <div class='feature-item'>
                                <span class='feature-emoji'>🎵</span> 
                                <span>Các playlist Lo-fi Chill mới nhất vừa được cập nhật.</span>
                            </div>
                            <div class='feature-item'>
                                <span class='feature-emoji'>⏱️</span> 
                                <span>Đồng hồ Pomodoro sẵn sàng cho phiên tập trung mới.</span>
                            </div>
                            <div class='feature-item'>
                                <span class='feature-emoji'>🌧️</span> 
                                <span>Âm thanh nền mưa rơi và tiếng quán cafe quen thuộc.</span>
                            </div>
                        </div>

                        <div class='btn-container'>
                            <a href='https://www.aestheticspace.live' class='btn'>Vào Bàn Học Ngay</a>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>Bạn nhận được email này vì bạn là thành viên của cộng đồng Aesthetic Study Space.</p>
                        <p>&copy; {DateTime.UtcNow.Year} Aesthetic Study Space. All rights reserved.</p>
                    </div>
                </div>
            </div>
        </body>
        </html>";
    }
}
