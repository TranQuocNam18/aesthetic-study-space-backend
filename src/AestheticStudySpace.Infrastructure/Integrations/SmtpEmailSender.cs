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
        if (string.IsNullOrWhiteSpace(_settings.Host)
            || string.IsNullOrWhiteSpace(_settings.FromEmail)
            || !_settings.FromEmail.Contains('@'))
        {
            _logger.LogWarning(
                "SMTP is not configured properly (Host='{Host}', FromEmail='{FromEmail}'). Skipping email to {ToEmail} with subject '{Subject}'.",
                _settings.Host, _settings.FromEmail, toEmail, subject);
            return;
        }

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
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        // SmtpClient lacks true cancellation support; best-effort.
        await client.SendMailAsync(message, cancellationToken);
    }
}

