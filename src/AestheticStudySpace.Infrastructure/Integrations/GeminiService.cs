using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateWelcomeMessageAsync(
        string username,
        int completedTasksCount,
        int totalFocusMinutes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey.Equals("YOUR_GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Gemini API key is not configured or using default placeholder. Using fallback welcome message.");
            return GetFallbackMessage(username, completedTasksCount, totalFocusMinutes);
        }

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var prompt = $"Chào mừng người dùng tên '{username}' quay lại ứng dụng học tập 'Aesthetic Study Space'. " +
                         $"Hôm qua (ngày trước đó), họ đã hoàn thành {completedTasksCount} công việc (nhiệm vụ) " +
                         $"và có tổng cộng {totalFocusMinutes} phút tập trung bằng đồng hồ Pomodoro. " +
                         "Hãy tạo một câu chào mừng cá nhân hóa, khích lệ và truyền cảm hứng bằng tiếng Việt. " +
                         "Yêu cầu: " +
                         "1. Xưng hô là 'bạn'. " +
                         "2. Nội dung ngắn gọn (tối đa 3 câu). " +
                         "3. Nhắc đến số phút tập trung và số nhiệm vụ họ đã làm ngày hôm qua để khích lệ. " +
                         "4. Tuyệt đối không trả về bất kỳ định dạng markdown nào, chỉ trả về chuỗi văn bản thuần túy.";

            var requestBody = new GeminiRequest
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Parts = new[]
                        {
                            new GeminiPart { Text = prompt }
                        }
                    }
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                return GetFallbackMessage(username, completedTasksCount, totalFocusMinutes);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseContent, jsonOptions);

            var text = result?.Candidates?[0]?.Content?.Parts?[0]?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            _logger.LogWarning("Gemini API returned an empty or invalid response. Using fallback welcome message.");
            return GetFallbackMessage(username, completedTasksCount, totalFocusMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Gemini API: {Message}", ex.Message);
            return GetFallbackMessage(username, completedTasksCount, totalFocusMinutes);
        }
    }

    private string GetFallbackMessage(string username, int completedTasksCount, int totalFocusMinutes)
    {
        if (totalFocusMinutes > 0 || completedTasksCount > 0)
        {
            return $"Chào mừng bạn trở lại, {username}! Hôm qua bạn đã có {totalFocusMinutes} phút tập trung tuyệt vời và hoàn thành {completedTasksCount} nhiệm vụ. Hãy tiếp tục duy trì phong độ này hôm nay nhé!";
        }
        return $"Chào mừng bạn trở lại, {username}! Hãy thiết lập các mục tiêu học tập hôm nay và bắt đầu không gian âm nhạc để tập trung cao độ nhé!";
    }

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiResponseContent? Content { get; set; }
    }

    private class GeminiResponseContent
    {
        [JsonPropertyName("parts")]
        public GeminiResponsePart[]? Parts { get; set; }
    }

    private class GeminiResponsePart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
