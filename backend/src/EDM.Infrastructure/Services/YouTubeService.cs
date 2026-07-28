using EDM.Application.Interfaces.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using YtService = Google.Apis.YouTube.v3.YouTubeService;

namespace EDM.Infrastructure.Services;

public class YouTubeService : IYouTubeService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IS3Service _s3Service;
    private readonly ILogger<YouTubeService> _logger;

    public YouTubeService(
        HttpClient httpClient,
        IConfiguration configuration,
        IS3Service s3Service,
        ILogger<YouTubeService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<string> UploadVideoAsync(
        string s3Key, string title, string description, CancellationToken cancellationToken)
    {
        var ytService = CreateYtService();

        // S3'ten video akışını al (presigned URL üzerinden)
        var presignedUrl = _s3Service.GetPresignedUrl(s3Key, TimeSpan.FromHours(2));
        await using var videoStream = await _httpClient.GetStreamAsync(presignedUrl, cancellationToken);

        var video = new Video
        {
            Snippet = new VideoSnippet
            {
                Title = title.Length > 100 ? title[..100] : title,
                Description = description,
                CategoryId = "10", // Music
                DefaultLanguage = "en",
                Tags = ExtractHashtags(description)
            },
            Status = new VideoStatus
            {
                PrivacyStatus = "public",
                SelfDeclaredMadeForKids = false
            }
        };

        var insertRequest = ytService.Videos.Insert(video, "snippet,status", videoStream, "video/mp4");
        insertRequest.NotifySubscribers = true;
        insertRequest.ChunkSize = ResumableUpload.MinimumChunkSize * 4;

        insertRequest.ProgressChanged += progress =>
            _logger.LogDebug("YouTube yükleme: {Bytes} bytes, durum: {Status}", progress.BytesSent, progress.Status);

        var result = await insertRequest.UploadAsync(cancellationToken);
        if (result.Status == UploadStatus.Failed)
            throw new InvalidOperationException($"YouTube upload başarısız: {result.Exception?.Message}");

        var videoId = insertRequest.ResponseBody?.Id
            ?? throw new InvalidOperationException("YouTube video ID alınamadı");

        _logger.LogInformation("YouTube videosu yüklendi: https://youtu.be/{VideoId}", videoId);
        return videoId;
    }

    private YtService CreateYtService()
    {
        var clientId = _configuration["YouTube:ClientId"]
            ?? throw new InvalidOperationException("YouTube:ClientId is not configured");
        var clientSecret = _configuration["YouTube:ClientSecret"]
            ?? throw new InvalidOperationException("YouTube:ClientSecret is not configured");
        var refreshToken = _configuration["YouTube:RefreshToken"]
            ?? throw new InvalidOperationException("YouTube:RefreshToken is not configured");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = new[] { YtService.Scope.YoutubeUpload }
        });

        var credential = new UserCredential(flow, "user", new TokenResponse { RefreshToken = refreshToken });

        return new YtService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "EDM Journal"
        });
    }

    private static IList<string> ExtractHashtags(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.StartsWith('#') && w.Length > 1)
            .Select(w => w[1..])
            .ToList();
    }
}
