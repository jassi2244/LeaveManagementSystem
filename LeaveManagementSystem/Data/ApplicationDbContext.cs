using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveAllocation> LeaveAllocations => Set<LeaveAllocation>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ProfilePicture).HasMaxLength(255);
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Manager)
                .WithMany(m => m.TeamMembers)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LeaveAllocation>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.LeaveAllocations)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.LeaveType)
                .WithMany(l => l.LeaveAllocations)
                .HasForeignKey(e => e.LeaveTypeId);
            entity.HasIndex(e => new { e.UserId, e.LeaveTypeId, e.Period }).IsUnique();
        });

        builder.Entity<LeaveRequest>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.RequestingEmployee)
                .WithMany(u => u.RequestedLeaves)
                .HasForeignKey(e => e.RequestingEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ApprovedBy)
                .WithMany(u => u.ActionedLeaves)
                .HasForeignKey(e => e.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.LeaveType)
                .WithMany(l => l.LeaveRequests)
                .HasForeignKey(e => e.LeaveTypeId);
        });
    }
}
