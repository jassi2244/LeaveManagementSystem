using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class HolidayVM
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
}
