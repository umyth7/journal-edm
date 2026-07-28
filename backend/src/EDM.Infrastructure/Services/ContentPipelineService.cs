using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class ContentPipelineService : IContentPipelineService
{
    private readonly IContentJobRepository _contentJobRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly IS3Service _s3Service;
    private readonly ICaptionService _captionService;
    private readonly ICaptionRepository _captionRepository;
    private readonly ISchedulingService _schedulingService;
    private readonly IInstagramService _instagramService;
    private readonly ITikTokService _tikTokService;
    private readonly IYouTubeService _youTubeService;
    private readonly ILogger<ContentPipelineService> _logger;

    public ContentPipelineService(
        IContentJobRepository contentJobRepository,
        IArtistRepository artistRepository,
        IGoogleDriveService googleDriveService,
        IS3Service s3Service,
        ICaptionService captionService,
        ICaptionRepository captionRepository,
        ISchedulingService schedulingService,
        IInstagramService instagramService,
        ITikTokService tikTokService,
        IYouTubeService youTubeService,
        ILogger<ContentPipelineService> logger)
    {
        _contentJobRepository = contentJobRepository;
        _artistRepository = artistRepository;
        _googleDriveService = googleDriveService;
        _s3Service = s3Service;
        _captionService = captionService;
        _captionRepository = captionRepository;
        _schedulingService = schedulingService;
        _instagramService = instagramService;
        _tikTokService = tikTokService;
        _youTubeService = youTubeService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid contentJobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pipeline başlatılıyor: {ContentJobId}", contentJobId);

        var job = await _contentJobRepository.GetByIdAsync(contentJobId, cancellationToken)
            ?? throw new InvalidOperationException($"ContentJob bulunamadı: {contentJobId}");

        var artist = await _artistRepository.GetByIdAsync(job.ArtistId, cancellationToken)
            ?? throw new InvalidOperationException($"Artist bulunamadı: {job.ArtistId}");

        job.Status = ContentJobStatus.Processing;
        await _contentJobRepository.UpdateAsync(job, cancellationToken);

        try
        {
            // === ADIM 1: Drive → S3 ===
            var s3Key = $"videos/{job.Year}/{job.EventName.ToLower().Replace(" ", "-")}/{artist.Slug}.mp4";

            if (!string.IsNullOrEmpty(job.RawFileName) && job.RawFileName.StartsWith("https://drive.google.com"))
            {
                _logger.LogInformation("Drive'dan indiriliyor: {DriveLink}", job.RawFileName);
                await using var fileStream = await _googleDriveService.DownloadFileAsync(job.RawFileName, cancellationToken);
                await _s3Service.UploadAsync(fileStream, s3Key, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Drive linki yok, S3 yüklemesi atlandı. RawFileName: {RawFileName}", job.RawFileName);
            }

            job.S3Key = s3Key;
            await _contentJobRepository.UpdateAsync(job, cancellationToken);

            // === ADIM 2: Caption üret (3 platform paralel) ===
            _logger.LogInformation("Caption üretiliyor: {Artist} @ {Event} {Year}", artist.Name, job.EventName, job.Year);
            var captions = await _captionService.GenerateAllPlatformsAsync(
                artist.Name, job.EventName, job.Year, contentJobId, cancellationToken);

            foreach (var caption in captions)
                await _captionRepository.CreateAsync(caption, cancellationToken);

            job.Status = ContentJobStatus.CaptionGenerated;

            // === ADIM 3: Yayın zamanı hesapla ===
            var scheduledAt = _schedulingService.CalculatePrimeTimeSlot(artist.Score, Platform.Instagram, DateTime.UtcNow.AddDays(1));
            job.ScheduledAt = scheduledAt;
            job.Status = ContentJobStatus.Scheduled;
            await _contentJobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation("İçerik zamanlandı: {ContentJobId}, ScheduledAt: {ScheduledAt}", contentJobId, scheduledAt);

            // === ADIM 4: Platform yayınları (zamanlanmış) ===
            // Not: Gerçek ortamda bu Hangfire ile scheduledAt anında tetiklenir.
            // Geliştirme ortamında veya manuel test için direkt çağrılır.
            var igCaption = captions.FirstOrDefault(c => c.Platform == Platform.Instagram);
            var ttCaption = captions.FirstOrDefault(c => c.Platform == Platform.TikTok);
            var ytCaption = captions.FirstOrDefault(c => c.Platform == Platform.YouTube);

            await PublishToAllPlatformsAsync(job, artist, igCaption?.CaptionText, ttCaption?.CaptionText, ytCaption, cancellationToken);

            job.Status = ContentJobStatus.Published;
            job.PublishedAt = DateTime.UtcNow;
            await _contentJobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation("Pipeline tamamlandı: {ContentJobId}", contentJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline hatası: {ContentJobId}", contentJobId);
            job.Status = ContentJobStatus.Failed;
            job.RetryCount++;
            await _contentJobRepository.UpdateAsync(job, cancellationToken);
            throw;
        }
    }

    private async Task PublishToAllPlatformsAsync(
        Domain.Entities.ContentJob job,
        Domain.Entities.Artist artist,
        string? igCaption,
        string? ttCaption,
        Domain.Entities.Caption? ytCaption,
        CancellationToken ct)
    {
        var tasks = new List<Task>();

        if (!string.IsNullOrEmpty(igCaption))
        {
            tasks.Add(PublishInstagramAsync(job, igCaption, ct));
        }

        if (!string.IsNullOrEmpty(ttCaption))
        {
            tasks.Add(PublishTikTokAsync(job, ttCaption, ct));
        }

        if (ytCaption != null)
        {
            var ytTitle = $"{artist.Name} @ {job.EventName} {job.Year} | Full Set";
            tasks.Add(PublishYouTubeAsync(job, ytTitle, ytCaption.CaptionText, ct));
        }

        await Task.WhenAll(tasks);
        await _contentJobRepository.UpdateAsync(job, ct);
    }

    private async Task PublishInstagramAsync(Domain.Entities.ContentJob job, string caption, CancellationToken ct)
    {
        try
        {
            var postId = await _instagramService.PublishReelAsync(job.S3Key, caption, ct);
            job.InstagramPostId = postId;
            _logger.LogInformation("Instagram yayını başarılı: {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Instagram yayın hatası: {ContentJobId}", job.Id);
        }
    }

    private async Task PublishTikTokAsync(Domain.Entities.ContentJob job, string caption, CancellationToken ct)
    {
        try
        {
            var postId = await _tikTokService.PublishVideoAsync(job.S3Key, caption, ct);
            job.TikTokPostId = postId;
            _logger.LogInformation("TikTok yayını başarılı: {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok yayın hatası: {ContentJobId}", job.Id);
        }
    }

    private async Task PublishYouTubeAsync(Domain.Entities.ContentJob job, string title, string description, CancellationToken ct)
    {
        try
        {
            var videoId = await _youTubeService.UploadVideoAsync(job.S3Key, title, description, ct);
            job.YoutubeVideoId = videoId;
            _logger.LogInformation("YouTube yüklemesi başarılı: {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube yükleme hatası: {ContentJobId}", job.Id);
        }
    }
}
