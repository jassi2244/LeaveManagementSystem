namespace LeaveManagementSystem.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
