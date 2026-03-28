using AutoMapper;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<LeaveType, LeaveTypeVM>().ReverseMap();
        CreateMap<Department, DepartmentVM>().ReverseMap();
        CreateMap<Holiday, HolidayVM>().ReverseMap();
        CreateMap<LeaveRequest, LeaveRequestDTO>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.RequestingEmployee != null ? s.RequestingEmployee.FullName : string.Empty))
            .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.RequestingEmployee != null && s.RequestingEmployee.Department != null ? s.RequestingEmployee.Department.Name : string.Empty))
            .ForMember(d => d.LeaveTypeName, opt => opt.MapFrom(s => s.LeaveType != null ? s.LeaveType.Name : string.Empty));
        CreateMap<LeaveRequest, LeaveRequestVM>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.RequestingEmployee != null ? s.RequestingEmployee.FullName : string.Empty))
            .ForMember(d => d.LeaveTypeName, opt => opt.MapFrom(s => s.LeaveType != null ? s.LeaveType.Name : string.Empty));
        CreateMap<ApplyLeaveVM, LeaveRequest>();
        CreateMap<LeaveBalanceVM, LeaveBalanceDTO>();
        CreateMap<LeaveReportDTO, LeaveRequestVM>();
    }
}
