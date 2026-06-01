using System.Net;
using System.Net.Mail;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Validate SMTP configuration
        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("SMTP Host is not configured. Set Smtp:Host environment variable.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("SMTP FromEmail is not configured. Set Smtp:FromEmail environment variable.");

        if (!_settings.FromEmail.Contains('@'))
            throw new InvalidOperationException($"SMTP FromEmail '{_settings.FromEmail}' is not a valid email address.");

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Timeout = 10000  // 10 seconds timeout
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username))
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

            _logger.LogInformation(
                "Sending email to {ToEmail} via SMTP {Host}:{Port} from {FromEmail}",
                toEmail, _settings.Host, _settings.Port, _settings.FromEmail);

            // SmtpClient lacks true cancellation support; best-effort.
            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "SMTP error sending email to {ToEmail}. SmtpStatusCode: {StatusCode}. Details: {Message}",
                toEmail, ex.StatusCode, ex.Message);
            throw new InvalidOperationException($"Failed to send email to {toEmail}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {ToEmail}: {Message}", toEmail, ex.Message);
            throw;
        }
    }
}

