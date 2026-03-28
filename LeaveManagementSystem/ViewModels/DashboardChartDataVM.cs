namespace LeaveManagementSystem.ViewModels;

public class DashboardChartDataVM
{
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public List<int> MonthlyRequestCounts { get; set; } = new();
}
