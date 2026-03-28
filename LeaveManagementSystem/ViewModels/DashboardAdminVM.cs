namespace LeaveManagementSystem.ViewModels;

public class DashboardAdminVM
{
    public int TotalEmployees { get; set; }
    public int TotalPendingRequests { get; set; }
    public int TotalApprovedThisMonth { get; set; }
    public int TotalRejectedThisMonth { get; set; }
    public List<LeaveRequestVM> RecentPendingRequests { get; set; } = new();
}
