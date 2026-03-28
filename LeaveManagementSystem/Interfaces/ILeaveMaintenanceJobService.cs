namespace LeaveManagementSystem.Interfaces;

public interface ILeaveMaintenanceJobService
{
    Task SendDailyLeaveSummaryAsync();
    Task ResetYearlyLeaveBalanceAsync();
}
