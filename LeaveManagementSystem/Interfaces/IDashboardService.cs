using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface IDashboardService
{
    Task<DashboardAdminVM> GetAdminDashboardAsync(int year);
    Task<DashboardManagerVM> GetManagerDashboardAsync(string managerId, int year);
    Task<DashboardEmployeeVM> GetEmployeeDashboardAsync(string userId, int year);
    Task<DashboardChartDataVM> GetAdminChartDataAsync(int year);
}
