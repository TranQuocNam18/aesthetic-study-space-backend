using System.Text;
using System.Text.Json;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class ResendEmailSender : IEmailSender
{
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly HttpClient _httpClient;

    private const string ResendApiUrl = "https://api.resend.com/emails";

    public ResendEmailSender(
        IOptions<ResendSettings> settings,
        ILogger<ResendEmailSender> logger,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Validate configuration
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new ValidationException("Resend API key is not configured. Please contact support.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new ValidationException("Resend sender email is not configured. Please contact support.");

        if (!_settings.FromEmail.Contains('@'))
            throw new ValidationException("Resend sender email configuration is invalid. Please contact support.");

        try
        {
            _logger.LogInformation(
                "Sending email to {ToEmail} via Resend from {FromEmail}",
                toEmail, _settings.FromEmail);

            var request = new ResendEmailRequest
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                To = toEmail,
                Subject = subject,
                Html = htmlBody
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Add Resend API key authentication
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var response = await _httpClient.PostAsync(ResendApiUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Resend API error sending email to {ToEmail}. StatusCode: {StatusCode}. Response: {Response}",
                    toEmail, response.StatusCode, errorContent);

                throw new ValidationException(
                    $"Failed to send email. Please try again later. (Error: {response.StatusCode})");
            }

            _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error sending email via Resend to {ToEmail}: {Message}", toEmail, ex.Message);
            throw new ValidationException("Failed to send email due to network error. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email via Resend to {ToEmail}: {Message}", toEmail, ex.Message);
            throw new ValidationException("Failed to send email. Please try again later.");
        }
    }

    private record ResendEmailRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("from")]
        public string From { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("to")]
        public string To { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("subject")]
        public string Subject { get; init; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("html")]
        public string Html { get; init; } = string.Empty;
    }
}
