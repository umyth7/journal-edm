using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDM.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class InstagramService : IInstagramService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IS3Service _s3Service;
    private readonly ILogger<InstagramService> _logger;

    private const string GraphApiBase = "https://graph.facebook.com/v19.0";

    public InstagramService(
        HttpClient httpClient,
        IConfiguration configuration,
        IS3Service s3Service,
        ILogger<InstagramService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<string> PublishReelAsync(string s3Key, string caption, CancellationToken cancellationToken)
    {
        var userId = _configuration["Instagram:UserId"]
            ?? throw new InvalidOperationException("Instagram:UserId is not configured");
        var accessToken = _configuration["Instagram:AccessToken"]
            ?? throw new InvalidOperationException("Instagram:AccessToken is not configured");

        // Instagram must download video from a public URL — use 1-hour presigned URL
        var videoUrl = _s3Service.GetPresignedUrl(s3Key, TimeSpan.FromHours(1));

        // Step 1: Create media container
        var containerId = await CreateReelContainerAsync(userId, accessToken, videoUrl, caption, cancellationToken);
        _logger.LogInformation("Instagram container oluşturuldu: {ContainerId}", containerId);

        // Step 2: Wait for video processing (max 5 min)
        await WaitForContainerReadyAsync(userId, accessToken, containerId, cancellationToken);

        // Step 3: Publish
        var postId = await PublishContainerAsync(userId, accessToken, containerId, cancellationToken);
        _logger.LogInformation("Instagram Reel yayınlandı: {PostId}", postId);
        return postId;
    }

    private async Task<string> CreateReelContainerAsync(
        string userId, string accessToken, string videoUrl, string caption, CancellationToken ct)
    {
        var url = $"{GraphApiBase}/{userId}/media" +
                  $"?media_type=REELS" +
                  $"&video_url={Uri.EscapeDataString(videoUrl)}" +
                  $"&caption={Uri.EscapeDataString(caption)}" +
                  $"&access_token={accessToken}";

        var response = await _httpClient.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphIdResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Instagram media container yanıtı boş");

        return result.Id;
    }

    private async Task WaitForContainerReadyAsync(
        string userId, string accessToken, string containerId, CancellationToken ct)
    {
        var maxAttempts = 30;
        for (var i = 0; i < maxAttempts; i++)
        {
            await Task.Delay(10_000, ct); // 10 sn bekle

            var url = $"{GraphApiBase}/{containerId}?fields=status_code&access_token={accessToken}";
            var response = await _httpClient.GetFromJsonAsync<ContainerStatusResponse>(url, ct);

            _logger.LogDebug("Instagram container durum: {Status}", response?.StatusCode);

            if (response?.StatusCode == "FINISHED") return;
            if (response?.StatusCode == "ERROR")
                throw new InvalidOperationException($"Instagram video işleme hatası: container {containerId}");
        }

        throw new TimeoutException("Instagram video işleme zaman aşımı (5 dk)");
    }

    private async Task<string> PublishContainerAsync(
        string userId, string accessToken, string containerId, CancellationToken ct)
    {
        var url = $"{GraphApiBase}/{userId}/media_publish" +
                  $"?creation_id={containerId}" +
                  $"&access_token={accessToken}";

        var response = await _httpClient.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphIdResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Instagram media_publish yanıtı boş");

        return result.Id;
    }

    private record GraphIdResponse([property: JsonPropertyName("id")] string Id);

    private record ContainerStatusResponse(
        [property: JsonPropertyName("status_code")] string? StatusCode,
        [property: JsonPropertyName("id")] string Id);
}
