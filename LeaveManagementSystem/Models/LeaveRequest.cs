namespace LeaveManagementSystem.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public string RequestingEmployeeId { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ApprovedById { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime? DateActioned { get; set; }
    public DateTime DateRequested { get; set; } = DateTime.UtcNow;
    public bool Cancelled { get; set; }

    public ApplicationUser? RequestingEmployee { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public LeaveType? LeaveType { get; set; }
}
