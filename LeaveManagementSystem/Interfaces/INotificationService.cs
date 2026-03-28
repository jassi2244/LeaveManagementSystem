namespace LeaveManagementSystem.Interfaces;

public interface INotificationService
{
    Task SendAsync(string recipientUserId, string subject, string body);
    Task SendEmailAsync(string recipientEmail, string subject, string body);
}
