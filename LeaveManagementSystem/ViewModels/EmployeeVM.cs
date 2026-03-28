namespace LeaveManagementSystem.ViewModels;

public class EmployeeVM
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; }
}
