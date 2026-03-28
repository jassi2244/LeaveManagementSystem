namespace LeaveManagementSystem.Models;

public class Holiday
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
