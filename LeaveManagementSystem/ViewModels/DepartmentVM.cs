using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class DepartmentVM
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
