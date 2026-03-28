using Hangfire;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailNotificationService _emailNotificationService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        UserManager<ApplicationUser> userManager,
        EmailNotificationService emailNotificationService,
        IBackgroundJobClient backgroundJobClient,
        ILogger<NotificationService> logger)
    {
        _userManager = userManager;
        _emailNotificationService = emailNotificationService;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public Task SendAsync(string recipientUserId, string subject, string body)
    {
        _backgroundJobClient.Enqueue<NotificationService>(x =>
            x.ProcessUserNotificationAsync(recipientUserId, subject, body));
        _logger.LogInformation("Notification job queued. RecipientUserId: {RecipientUserId}, Subject: {Subject}", recipientUserId, subject);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        _backgroundJobClient.Enqueue<NotificationService>(x =>
            x.ProcessEmailNotificationAsync(recipientEmail, subject, body));
        _logger.LogInformation("Email notification job queued. Recipient: {RecipientEmail}, Subject: {Subject}", recipientEmail, subject);
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 15, 30 })]
    public async Task ProcessUserNotificationAsync(string recipientUserId, string subject, string body)
    {
        var user = await _userManager.FindByIdAsync(recipientUserId);
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogWarning("Skipping notification. User/email not found for user id {RecipientUserId}", recipientUserId);
            return;
        }

        await _emailNotificationService.SendEmailAsync(user.Email, subject, body, CancellationToken.None);
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 15, 30 })]
    public async Task ProcessEmailNotificationAsync(string recipientEmail, string subject, string body)
    {
        await _emailNotificationService.SendEmailAsync(recipientEmail, subject, body, CancellationToken.None);
    }
}
