using Microsoft.AspNetCore.Identity;

namespace LeaveManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? ManagerId { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfilePicture { get; set; }

    public Department? Department { get; set; }
    public ApplicationUser? Manager { get; set; }
    public ICollection<ApplicationUser> TeamMembers { get; set; } = new List<ApplicationUser>();
    public ICollection<LeaveAllocation> LeaveAllocations { get; set; } = new List<LeaveAllocation>();
    public ICollection<LeaveRequest> RequestedLeaves { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveRequest> ActionedLeaves { get; set; } = new List<LeaveRequest>();
}
