using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface IReportService
{
    Task<List<LeaveReportDTO>> GetLeaveReportAsync(ReportFilterVM filter, string? managerId = null);
    Task<ReportSummaryDTO> GetReportSummaryAsync(ReportFilterVM filter, string? managerId = null);
}
