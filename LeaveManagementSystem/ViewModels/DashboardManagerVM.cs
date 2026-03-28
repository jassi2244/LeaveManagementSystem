namespace LeaveManagementSystem.ViewModels;

public class DashboardManagerVM
{
    public int TeamSize { get; set; }
    public int PendingApprovals { get; set; }
    public int ApprovedThisMonth { get; set; }
    public List<LeaveRequestVM> TeamPendingRequests { get; set; } = new();
}
