namespace LeaveManagementSystem.DTOs;

public class ReportSummaryDTO
{
    public int Total { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}
