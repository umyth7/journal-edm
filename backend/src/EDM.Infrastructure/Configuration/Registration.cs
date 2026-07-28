using EDM.Application.Interfaces.Infrastructure;
using EDM.Infrastructure.Jobs;
using EDM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EDM.Infrastructure.Configuration;

public static class Registration
{
    public static IServiceCollection UseInfrastructure(this IServiceCollection services)
    {
        // Sprint 1 — core
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IContentJobService, ContentJobService>();
        services.AddScoped<IFilenameParser, FilenameParser>();
        services.AddScoped<ISchedulingService, SchedulingService>();

        // Sprint 2 — Google
        services.AddScoped<IGmailService, GmailClientService>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();

        // Sprint 2 — AWS
        services.AddScoped<IS3Service, S3Service>();

        // Sprint 2 — Caption (Anthropic HTTP client)
        services.AddHttpClient<CaptionService>();
        services.AddScoped<ICaptionService, CaptionService>();

        // Sprint 2 — Pipeline & Jobs
        services.AddScoped<IContentPipelineService, ContentPipelineService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // Sprint 3 — Platform publishing (gerçek API)
        services.AddHttpClient<InstagramService>();
        services.AddScoped<IInstagramService, InstagramService>();

        services.AddHttpClient<TikTokService>();
        services.AddScoped<ITikTokService, TikTokService>();

        services.AddHttpClient<YouTubeService>();
        services.AddScoped<IYouTubeService, YouTubeService>();

        // Sprint 3 — Artist score engine
        services.AddScoped<IArtistScoreService, ArtistScoreService>();

        // Sprint 3 — Gmail polling job
        services.AddScoped<GmailPollingJob>();

        return services;
    }
}
