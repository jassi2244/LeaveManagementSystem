namespace LeaveManagementSystem.ViewModels;

public class LeaveAllocationVM
{
    public string UserId { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public int NumberOfDays { get; set; }
    public int Year { get; set; }
}
