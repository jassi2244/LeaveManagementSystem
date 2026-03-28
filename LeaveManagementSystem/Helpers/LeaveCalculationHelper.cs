using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Helpers;

public static class LeaveCalculationHelper
{
    public static int CalculateWorkingDays(DateTime startDate, DateTime endDate, List<Holiday> holidays)
    {
        if (endDate.Date < startDate.Date)
        {
            return 0;
        }

        var holidayDates = holidays.Select(x => x.HolidayDate.Date).ToHashSet();
        var workingDays = 0;

        for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
        {
            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                continue;
            }

            if (holidayDates.Contains(day))
            {
                continue;
            }

            workingDays++;
        }

        return workingDays;
    }
}
