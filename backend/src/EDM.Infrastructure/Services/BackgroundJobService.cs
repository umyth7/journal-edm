using EDM.Application.Interfaces.Infrastructure;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(ILogger<BackgroundJobService> logger)
    {
        _logger = logger;
    }

    public string EnqueueContentPipeline(Guid contentJobId)
    {
        var jobId = BackgroundJob.Enqueue<IContentPipelineService>(
            service => service.ExecuteAsync(contentJobId, CancellationToken.None));

        _logger.LogInformation("Pipeline kuyruğa alındı: ContentJobId={ContentJobId}, HangfireJobId={JobId}", contentJobId, jobId);
        return jobId;
    }

    public string SchedulePublication(Guid contentJobId, DateTime scheduledAt)
    {
        var delay = scheduledAt - DateTime.UtcNow;
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

        var jobId = BackgroundJob.Schedule<IInstagramService>(
            service => service.PublishReelAsync(contentJobId.ToString(), "", CancellationToken.None),
            delay);

        _logger.LogInformation("Yayın zamanlandı: ContentJobId={ContentJobId}, ScheduledAt={ScheduledAt}, JobId={JobId}",
            contentJobId, scheduledAt, jobId);
        return jobId;
    }
}
