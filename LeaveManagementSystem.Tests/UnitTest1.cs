using LeaveManagementSystem.Helpers;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Tests;

public class LeaveCalculationHelperTests
{
    [Fact]
    public void CalculateWorkingDays_ExcludesWeekendsAndHolidays()
    {
        var startDate = new DateTime(2026, 3, 23); // Monday
        var endDate = new DateTime(2026, 3, 27);   // Friday
        var holidays = new List<Holiday>
        {
            new() { HolidayDate = new DateTime(2026, 3, 25) } // Wednesday
        };

        var days = LeaveCalculationHelper.CalculateWorkingDays(startDate, endDate, holidays);

        Assert.Equal(4, days);
    }
}
