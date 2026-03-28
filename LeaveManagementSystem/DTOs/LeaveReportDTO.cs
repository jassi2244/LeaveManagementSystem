namespace LeaveManagementSystem.DTOs;

public class LeaveReportDTO
{
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}
