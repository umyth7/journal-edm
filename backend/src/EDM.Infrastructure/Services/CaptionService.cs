using System.Text;
using System.Text.Json;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Domain.Entities;
using EDM.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class CaptionService : ICaptionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CaptionService> _logger;
    private const string AnthropicApiUrl = "https://api.anthropic.com/v1/messages";

    public CaptionService(HttpClient httpClient, IConfiguration configuration, ILogger<CaptionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Caption> GenerateAsync(string artistName, string eventName, int year, Platform platform, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured");
        var model = _configuration["Anthropic:Model"] ?? "claude-opus-4-5";

        var prompt = BuildPrompt(artistName, eventName, year, platform);

        var requestBody = new
        {
            model,
            max_tokens = 1024,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, AnthropicApiUrl);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDoc = JsonDocument.Parse(responseContent);
        var captionText = jsonDoc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        var hashtags = ExtractHashtags(captionText);

        _logger.LogInformation("Caption uretildi: {Platform} - {Artist} @ {Event} {Year}", platform, artistName, eventName, year);

        return new Caption
        {
            Id = Guid.NewGuid(),
            Platform = platform,
            CaptionText = captionText,
            Hashtags = hashtags,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<List<Caption>> GenerateAllPlatformsAsync(string artistName, string eventName, int year, Guid contentJobId, CancellationToken cancellationToken)
    {
        var platforms = new[] { Platform.Instagram, Platform.TikTok, Platform.YouTube };
        var tasks = platforms.Select(p => GenerateAsync(artistName, eventName, year, p, cancellationToken));
        var captions = await Task.WhenAll(tasks);

        foreach (var caption in captions)
            caption.ContentJobId = contentJobId;

        return captions.ToList();
    }

    private static string BuildPrompt(string artistName, string eventName, int year, Platform platform)
    {
        return platform switch
        {
            Platform.Instagram =>
                $"Sen bir EDM muzik icerik uzmanisın. Asagidaki bilgilerle Instagram caption yaz:\n" +
                $"Sanatci: {artistName}\n" +
                $"Etkinlik: {eventName}\n" +
                $"Yil: {year}\n\n" +
                $"Format (AYNEN uygula):\n" +
                $"{artistName} @ {eventName} {year}\n\n" +
                $"[Sanatcinin stili ve set enerjisini anlatan 2-3 cumle]\n\n" +
                $"[25-30 hashtag - etkinlik, sanatci, genel EDM etiketleri]\n" +
                $"Zorunlu hashtagler: #{eventName}, #{eventName}{year}, #{artistName}, #EDM, #electronicmusic, #djset, #edmjournal, #rave, #festival",

            Platform.TikTok =>
                $"Sen bir EDM muzik icerik uzmanisın. TikTok caption yaz:\n" +
                $"Sanatci: {artistName}\n" +
                $"Etkinlik: {eventName}\n" +
                $"Yil: {year}\n\n" +
                $"Format (AYNEN uygula - tek satir):\n" +
                $"{artistName} @ {eventName} {year} [5-8 hashtag]\n" +
                $"Zorunlu hashtagler: #{artistName}, #{eventName}, #EDM, #djset, #fyp",

            Platform.YouTube =>
                $"Sen bir EDM muzik icerik uzmanisın. YouTube aciklamasi yaz:\n" +
                $"Sanatci: {artistName}\n" +
                $"Etkinlik: {eventName}\n" +
                $"Yil: {year}\n\n" +
                $"Baslik: {artistName} @ {eventName} {year} | Full Set\n\n" +
                $"Aciklama formati:\n" +
                $"- 3-4 cumle uzun aciklama (sanatci, etkinlik, set atmosferi)\n" +
                $"- Timestamps placeholder: 00:00 Intro, ...\n" +
                $"- 10-15 hashtag\n" +
                $"Zorunlu hashtagler: #{artistName}, #{eventName}, #{eventName}{year}, #EDM, #fullset, #djset, #electronicmusic, #rave, #festival, #edmjournal",

            _ => throw new ArgumentOutOfRangeException(nameof(platform))
        };
    }

    private static string ExtractHashtags(string text)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"#\w+");
        return string.Join(" ", matches.Select(m => m.Value));
    }
}
