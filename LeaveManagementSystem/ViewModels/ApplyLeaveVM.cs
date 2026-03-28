using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class ApplyLeaveVM
{
    [Required]
    public int LeaveTypeId { get; set; }
    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    [Required, DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    [StringLength(500)]
    public string? Reason { get; set; }
    public bool IsHalfDay { get; set; }
    public int CalculatedWorkingDays { get; set; }
}
