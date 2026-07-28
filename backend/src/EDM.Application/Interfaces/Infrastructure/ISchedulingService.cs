using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Infrastructure;

public interface ISchedulingService
{
    DateTime CalculatePrimeTimeSlot(int artistScore, Platform platform, DateTime baseDate);
}
