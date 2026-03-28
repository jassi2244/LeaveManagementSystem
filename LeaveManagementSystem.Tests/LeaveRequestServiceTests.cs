using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Options;
using LeaveManagementSystem.Services;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace LeaveManagementSystem.Tests;

public class LeaveRequestServiceTests
{
    private static UserManager<ApplicationUser> BuildUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new UserManager<ApplicationUser>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task ApplyLeave_ShouldFail_WhenOverlappingRequestExists()
    {
        var leaveRequestRepo = new Mock<ILeaveRequestRepository>();
        var leaveAllocationRepo = new Mock<ILeaveAllocationRepository>();
        var holidayRepo = new Mock<IHolidayRepository>();
        var notificationService = new Mock<INotificationService>();
        var userManager = BuildUserManager();

        leaveRequestRepo
            .Setup(x => x.HasOverlappingAsync("u1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);
        holidayRepo
            .Setup(x => x.GetBetweenAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Holiday>());

        var service = new LeaveRequestService(
            leaveRequestRepo.Object,
            leaveAllocationRepo.Object,
            holidayRepo.Object,
            notificationService.Object,
            Microsoft.Extensions.Options.Options.Create(new LeavePolicyOptions()),
            userManager,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<LeaveRequestService>.Instance);

        var result = await service.ApplyLeaveAsync(new ApplyLeaveVM
        {
            LeaveTypeId = 1,
            StartDate = DateTime.UtcNow.Date.AddDays(2),
            EndDate = DateTime.UtcNow.Date.AddDays(3)
        }, "u1");

        Assert.False(result.Success);
        Assert.Contains("Overlapping", result.Message);
    }

    [Fact]
    public async Task ApplyLeave_HalfDay_ShouldSucceed_ForSingleWorkingDay()
    {
        var leaveRequestRepo = new Mock<ILeaveRequestRepository>();
        var leaveAllocationRepo = new Mock<ILeaveAllocationRepository>();
        var holidayRepo = new Mock<IHolidayRepository>();
        var notificationService = new Mock<INotificationService>();
        var userManager = BuildUserManager();

        var applyDate = DateTime.UtcNow.Date.AddDays(2);
        leaveRequestRepo
            .Setup(x => x.HasOverlappingAsync("u1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        holidayRepo
            .Setup(x => x.GetBetweenAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Holiday>());
        leaveAllocationRepo
            .Setup(x => x.GetLeaveBalanceBySpAsync("u1", applyDate.Year))
            .ReturnsAsync(new List<LeaveBalanceVM>
            {
                new() { LeaveTypeId = 1, TotalDays = 12, UsedDays = 0, RemainingDays = 12, LeaveTypeName = "Casual" }
            });
        leaveAllocationRepo
            .Setup(x => x.GetAsync("u1", 1, applyDate.Year))
            .ReturnsAsync(new LeaveAllocation { LeaveTypeId = 1, UserId = "u1", Period = applyDate.Year, NumberOfDays = 12, UsedDays = 0 });
        leaveRequestRepo
            .Setup(x => x.ApplyBySpAsync("u1", 1, applyDate, applyDate, 1, null))
            .ReturnsAsync((1, "Applied"));

        var service = new LeaveRequestService(
            leaveRequestRepo.Object,
            leaveAllocationRepo.Object,
            holidayRepo.Object,
            notificationService.Object,
            Microsoft.Extensions.Options.Options.Create(new LeavePolicyOptions()),
            userManager,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<LeaveRequestService>.Instance);

        var result = await service.ApplyLeaveAsync(new ApplyLeaveVM
        {
            LeaveTypeId = 1,
            StartDate = applyDate,
            EndDate = applyDate,
            IsHalfDay = true
        }, "u1");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ApproveLeave_ByManager_ShouldMoveToPendingAdminApproval()
    {
        var leaveRequestRepo = new Mock<ILeaveRequestRepository>();
        var leaveAllocationRepo = new Mock<ILeaveAllocationRepository>();
        var holidayRepo = new Mock<IHolidayRepository>();
        var notificationService = new Mock<INotificationService>();

        var manager = new ApplicationUser { Id = "m1", Email = "m1@lms.com", UserName = "m1@lms.com" };
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock.Setup(x => x.FindByIdAsync("m1")).ReturnsAsync(manager);
        userManagerMock.Setup(x => x.IsInRoleAsync(manager, "Admin")).ReturnsAsync(false);
        userManagerMock.Setup(x => x.IsInRoleAsync(manager, "Manager")).ReturnsAsync(true);

        leaveRequestRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new LeaveRequest
        {
            Id = 10,
            RequestingEmployeeId = "e1",
            Status = "Pending"
        });

        var service = new LeaveRequestService(
            leaveRequestRepo.Object,
            leaveAllocationRepo.Object,
            holidayRepo.Object,
            notificationService.Object,
            Microsoft.Extensions.Options.Options.Create(new LeavePolicyOptions()),
            userManagerMock.Object,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<LeaveRequestService>.Instance);

        var result = await service.ApproveLeaveAsync(10, "m1", "ok");

        Assert.True(result.Success);
        leaveRequestRepo.Verify(x => x.UpdateAsync(It.Is<LeaveRequest>(r => r.Status == "PendingAdminApproval")), Times.Once);
    }
}
