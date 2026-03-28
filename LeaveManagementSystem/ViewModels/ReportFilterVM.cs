namespace LeaveManagementSystem.ViewModels;

public class ReportFilterVM
{
    public int? DepartmentId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Year { get; set; }
    public string? EmployeeName { get; set; }
    public List<LeaveRequestVM> Records { get; set; } = new();
}
