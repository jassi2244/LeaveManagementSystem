namespace LeaveManagementSystem.ViewModels;

public class LeaveBalanceVM
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
}
