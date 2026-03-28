using LeaveManagementSystem.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LeaveManagementSystem.Services;

public class EmailNotificationService
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IOptions<SmtpOptions> smtpOptions, ILogger<EmailNotificationService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        var retries = Math.Max(1, _smtpOptions.MaxRetryAttempts);
        var delaySeconds = Math.Max(1, _smtpOptions.RetryDelaySeconds);

        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(recipientEmail);

                using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
                {
                    EnableSsl = _smtpOptions.EnableSsl,
                    Credentials = new NetworkCredential(_smtpOptions.UserName, _smtpOptions.Password)
                };

                cancellationToken.ThrowIfCancellationRequested();
                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent successfully. Recipient: {Recipient}, Subject: {Subject}", recipientEmail, subject);
                return;
            }
            catch (Exception ex) when (attempt < retries)
            {
                _logger.LogWarning(ex, "Email send failed on attempt {Attempt}/{Retries}. Recipient: {Recipient}", attempt, retries, recipientEmail);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds * attempt), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email send failed permanently after {Retries} retries. Recipient: {Recipient}", retries, recipientEmail);
                throw;
            }
        }
    }
}
