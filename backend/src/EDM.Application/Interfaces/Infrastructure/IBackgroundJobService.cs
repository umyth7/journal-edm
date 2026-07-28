namespace EDM.Application.Interfaces.Infrastructure;

public interface IBackgroundJobService
{
    string EnqueueContentPipeline(Guid contentJobId);
    string SchedulePublication(Guid contentJobId, DateTime scheduledAt);
}
