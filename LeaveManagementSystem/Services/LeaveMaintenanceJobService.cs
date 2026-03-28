using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Services;

public class LeaveMaintenanceJobService : ILeaveMaintenanceJobService
{
    private readonly IReportService _reportService;
    private readonly INotificationService _notificationService;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LeaveMaintenanceJobService> _logger;

    public LeaveMaintenanceJobService(
        IReportService reportService,
        INotificationService notificationService,
        ILeaveAllocationRepository leaveAllocationRepository,
        IConfiguration configuration,
        ILogger<LeaveMaintenanceJobService> logger)
    {
        _reportService = reportService;
        _notificationService = notificationService;
        _leaveAllocationRepository = leaveAllocationRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendDailyLeaveSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var filter = new ReportFilterVM
        {
            FromDate = today,
            ToDate = today
        };

        var rows = await _reportService.GetLeaveReportAsync(filter);
        var summaryBody = $"""
                           Daily leave summary for {today:dd MMM yyyy}

                           Total requests: {rows.Count}
                           Approved: {rows.Count(x => x.Status == "Approved")}
                           Pending: {rows.Count(x => x.Status == "Pending")}
                           Rejected: {rows.Count(x => x.Status == "Rejected")}
                           """;

        var adminEmail = _configuration["AppSettings:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            _logger.LogWarning("Daily summary skipped because AppSettings:AdminEmail is missing.");
            return;
        }

        await _notificationService.SendEmailAsync(adminEmail, "Daily leave summary", summaryBody.Replace(Environment.NewLine, "<br/>"));
    }

    public async Task ResetYearlyLeaveBalanceAsync()
    {
        var year = DateTime.UtcNow.Year;
        var updated = await _leaveAllocationRepository.ResetYearlyUsedDaysAsync(year);
        _logger.LogInformation("Yearly leave usage reset completed for {Year}. Rows updated: {RowsUpdated}", year, updated);
    }
}
