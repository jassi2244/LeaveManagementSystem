using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class RegisterVM
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = "Employee";
    public int? DepartmentId { get; set; }
    public string? ManagerId { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateOfJoining { get; set; }
}
