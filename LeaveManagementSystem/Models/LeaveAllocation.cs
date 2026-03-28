namespace LeaveManagementSystem.Models;

public class LeaveAllocation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public int NumberOfDays { get; set; }
    public int UsedDays { get; set; }
    public int Period { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public int RemainingDays => NumberOfDays - UsedDays;

    public ApplicationUser? User { get; set; }
    public LeaveType? LeaveType { get; set; }
}
