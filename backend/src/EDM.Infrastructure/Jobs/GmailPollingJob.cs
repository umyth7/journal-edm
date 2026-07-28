using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Jobs;

public class GmailPollingJob
{
    private readonly IGmailService _gmailService;
    private readonly IFilenameParser _filenameParser;
    private readonly IArtistRepository _artistRepository;
    private readonly IContentJobRepository _contentJobRepository;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<GmailPollingJob> _logger;

    public GmailPollingJob(
        IGmailService gmailService,
        IFilenameParser filenameParser,
        IArtistRepository artistRepository,
        IContentJobRepository contentJobRepository,
        IBackgroundJobService backgroundJobService,
        ILogger<GmailPollingJob> logger)
    {
        _gmailService = gmailService;
        _filenameParser = filenameParser;
        _artistRepository = artistRepository;
        _contentJobRepository = contentJobRepository;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gmail polling başladı");

        var messages = await _gmailService.GetNewMessagesAsync(cancellationToken);
        _logger.LogInformation("Okunmamış {Count} mail bulundu", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                if (string.IsNullOrEmpty(message.DriveLink))
                {
                    _logger.LogWarning("Mail {MessageId} için Drive linki yok, atlanıyor", message.MessageId);
                    await _gmailService.MarkAsReadAsync(message.MessageId, cancellationToken);
                    continue;
                }

                var fileName = message.FileName ?? ExtractFileNameFromSubject(message.Subject);
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("Mail {MessageId} için dosya adı tespit edilemedi", message.MessageId);
                    continue;
                }

                // Format: {YYYY}_{EVENT}_{ARTIST}.ext
                var parsed = ParseEdmFileName(fileName);
                if (!parsed.HasValue)
                {
                    _logger.LogWarning("Dosya adı parse edilemedi: {FileName}", fileName);
                    await _gmailService.MarkAsReadAsync(message.MessageId, cancellationToken);
                    continue;
                }

                var (year, eventName, artistName) = parsed.Value;
                var artistSlug = Slugify(artistName);

                // Artist bul veya oluştur
                var artist = await _artistRepository.GetBySlugAsync(artistSlug, cancellationToken);
                if (artist == null)
                {
                    artist = new Artist
                    {
                        Id = Guid.NewGuid(),
                        Name = artistName,
                        Slug = artistSlug,
                        Score = 50,
                        LastScoreUpdate = DateTime.UtcNow
                    };
                    artist = await _artistRepository.CreateAsync(artist, cancellationToken);
                    _logger.LogInformation("Yeni artist oluşturuldu: {Name} (slug: {Slug})", artistName, artistSlug);
                }

                // ContentJob oluştur
                var job = new Domain.Entities.ContentJob
                {
                    Id = Guid.NewGuid(),
                    ArtistId = artist.Id,
                    Year = year,
                    EventName = eventName,
                    RawFileName = message.DriveLink, // Pipeline DriveLink'ten S3'e indirir
                    Status = Domain.Enums.ContentJobStatus.Pending
                };

                job = await _contentJobRepository.CreateAsync(job, cancellationToken);
                _logger.LogInformation("ContentJob oluşturuldu: {JobId} | {Artist} @ {Event} {Year}",
                    job.Id, artistName, eventName, year);

                // Pipeline'ı kuyruğa al
                _backgroundJobService.EnqueueContentPipeline(job.Id);

                // Maili okundu işaretle
                await _gmailService.MarkAsReadAsync(message.MessageId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail işlenirken hata: {MessageId}", message.MessageId);
            }
        }

        _logger.LogInformation("Gmail polling tamamlandı");
    }

    // "2026_Tomorrowland_R3hab.mp4" → (2026, "Tomorrowland", "R3hab")
    // "2025_Ultra_Martin_Garrix.mov" → (2025, "Ultra", "Martin Garrix")
    private static (int year, string eventName, string artistName)? ParseEdmFileName(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExt.Split('_');

        if (parts.Length < 3) return null;
        if (!int.TryParse(parts[0], out var year) || year < 2000 || year > 2100) return null;

        var eventName = Capitalize(parts[1]);

        // Son token(lar) sanatçı adı
        var artistParts = parts[2..].Select(Capitalize);
        var artistName = string.Join(" ", artistParts);

        return (year, eventName, artistName);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    private static string Slugify(string name) =>
        name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

    private static string? ExtractFileNameFromSubject(string subject)
    {
        // Subject'ten .mp4/.mov uzantılı kelime ara
        var words = subject.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.FirstOrDefault(w =>
            w.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
            w.EndsWith(".mov", StringComparison.OrdinalIgnoreCase));
    }
}
