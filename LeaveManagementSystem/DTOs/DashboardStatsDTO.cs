namespace LeaveManagementSystem.DTOs;

public class DashboardStatsDTO
{
    public int TotalEmployees { get; set; }
    public int TotalPendingRequests { get; set; }
    public int TotalApprovedThisMonth { get; set; }
    public int TotalRejectedThisMonth { get; set; }
    public List<KeyValuePair<string, int>> LeavesByDepartment { get; set; } = new();
    public List<KeyValuePair<int, int>> MonthlyLeaveCount { get; set; } = new();
}
