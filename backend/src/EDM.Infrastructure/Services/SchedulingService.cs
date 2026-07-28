using EDM.Application.Interfaces.Infrastructure;
using EDM.Domain.Enums;

namespace EDM.Infrastructure.Services;

public class SchedulingService : ISchedulingService
{
    public DateTime CalculatePrimeTimeSlot(int artistScore, Platform platform, DateTime baseDate)
    {
        // product.md prime time tablosu:
        // 80-100 -> 19:00-21:00
        // 50-79  -> 21:00-23:00
        // 20-49  -> 12:00-18:00
        // 1-19   -> 08:00-12:00

        var targetDate = baseDate.Date;
        TimeSpan selectedTime;

        if (artistScore >= 80)
            selectedTime = new TimeSpan(19, 0, 0);
        else if (artistScore >= 50)
            selectedTime = new TimeSpan(21, 0, 0);
        else if (artistScore >= 20)
            selectedTime = new TimeSpan(12, 0, 0);
        else
            selectedTime = new TimeSpan(8, 0, 0);

        return DateTime.SpecifyKind(targetDate + selectedTime, DateTimeKind.Utc);
    }
}
