namespace LeaveManagementSystem.ViewModels;

public class DashboardEmployeeVM
{
    public List<LeaveBalanceVM> LeaveBalances { get; set; } = new();
    public List<LeaveRequestVM> RecentRequests { get; set; } = new();
}
