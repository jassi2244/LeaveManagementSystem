using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class ApproveLeaveVM
{
    public int LeaveRequestId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    [StringLength(500)]
    public string? Comments { get; set; }
}
