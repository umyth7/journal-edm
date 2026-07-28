using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDM.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class TikTokService : ITikTokService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IS3Service _s3Service;
    private readonly ILogger<TikTokService> _logger;

    private const string TikTokApiBase = "https://open.tiktokapis.com/v2";

    public TikTokService(
        HttpClient httpClient,
        IConfiguration configuration,
        IS3Service s3Service,
        ILogger<TikTokService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<string> PublishVideoAsync(string s3Key, string caption, CancellationToken cancellationToken)
    {
        var accessToken = _configuration["TikTok:AccessToken"]
            ?? throw new InvalidOperationException("TikTok:AccessToken is not configured");

        // TikTok PULL_FROM_URL: TikTok indirir, presigned URL ile
        var videoUrl = _s3Service.GetPresignedUrl(s3Key, TimeSpan.FromHours(1));

        // Step 1: Init post
        var publishId = await InitPublishAsync(accessToken, videoUrl, caption, cancellationToken);
        _logger.LogInformation("TikTok publish başlatıldı: {PublishId}", publishId);

        // Step 2: Poll status
        var postId = await PollPublishStatusAsync(accessToken, publishId, cancellationToken);
        _logger.LogInformation("TikTok videosu yayınlandı: {PostId}", postId);
        return postId;
    }

    private async Task<string> InitPublishAsync(
        string accessToken, string videoUrl, string caption, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{TikTokApiBase}/post/publish/video/init/");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var body = new
        {
            post_info = new
            {
                title = caption.Length > 2200 ? caption[..2200] : caption,
                privacy_level = "PUBLIC_TO_EVERYONE",
                disable_duet = false,
                disable_comment = false,
                disable_stitch = false
            },
            source_info = new
            {
                source = "PULL_FROM_URL",
                video_url = videoUrl
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TikTokInitResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("TikTok init yanıtı boş");

        return result.Data.PublishId;
    }

    private async Task<string> PollPublishStatusAsync(string accessToken, string publishId, CancellationToken ct)
    {
        var maxAttempts = 20;
        for (var i = 0; i < maxAttempts; i++)
        {
            await Task.Delay(15_000, ct); // 15 sn bekle

            var request = new HttpRequestMessage(HttpMethod.Post, $"{TikTokApiBase}/post/publish/status/fetch/");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { publish_id = publishId }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) continue;

            var result = await response.Content.ReadFromJsonAsync<TikTokStatusResponse>(cancellationToken: ct);
            var status = result?.Data?.Status;
            _logger.LogDebug("TikTok publish durum: {Status}", status);

            if (status == "PUBLISH_COMPLETE")
                return result?.Data?.PublicityType ?? publishId;

            if (status == "FAILED")
                throw new InvalidOperationException($"TikTok video yayını başarısız: publishId={publishId}");
        }

        throw new TimeoutException("TikTok video işleme zaman aşımı (5 dk)");
    }

    private record TikTokInitResponse(
        [property: JsonPropertyName("data")] TikTokInitData Data,
        [property: JsonPropertyName("error")] TikTokError? Error);

    private record TikTokInitData(
        [property: JsonPropertyName("publish_id")] string PublishId);

    private record TikTokStatusResponse(
        [property: JsonPropertyName("data")] TikTokStatusData? Data);

    private record TikTokStatusData(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("publicity_type")] string? PublicityType);

    private record TikTokError(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("message")] string? Message);
}
