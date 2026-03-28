using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class LeaveTypeVM
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Range(1, 365)] public int DefaultDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
