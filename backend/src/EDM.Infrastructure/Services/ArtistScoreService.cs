using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class ArtistScoreService : IArtistScoreService
{
    private readonly IArtistRepository _artistRepository;
    private readonly ILogger<ArtistScoreService> _logger;

    // Normalizasyon eşikleri — bu değerler üzerindeki metrikler 100 puan alır
    private const long YoutubeViewsMax = 10_000_000L;
    private const long InstagramReachMax = 5_000_000L;
    private const long TikTokPlaysMax = 20_000_000L;
    private const long SpotifyListenersMax = 10_000_000L;

    public ArtistScoreService(IArtistRepository artistRepository, ILogger<ArtistScoreService> logger)
    {
        _artistRepository = artistRepository;
        _logger = logger;
    }

    public int CalculateScore(long youtubeViews90d, long instagramReach90d, long tiktokPlays90d, long spotifyListeners)
    {
        var ytScore = Normalize(youtubeViews90d, YoutubeViewsMax) * 0.35;
        var igScore = Normalize(instagramReach90d, InstagramReachMax) * 0.30;
        var ttScore = Normalize(tiktokPlays90d, TikTokPlaysMax) * 0.25;
        var spScore = Normalize(spotifyListeners, SpotifyListenersMax) * 0.10;

        var total = ytScore + igScore + ttScore + spScore;
        return Math.Max(1, Math.Min(100, (int)Math.Round(total)));
    }

    public async Task RecalculateAsync(Guid artistId, CancellationToken cancellationToken)
    {
        var artist = await _artistRepository.GetByIdAsync(artistId, cancellationToken);
        if (artist == null)
        {
            _logger.LogWarning("Score hesaplama: artist bulunamadı {ArtistId}", artistId);
            return;
        }

        if (artist.ManualOverride)
        {
            _logger.LogDebug("Artist {Name} manuel override aktif, skor hesaplanmadı", artist.Name);
            return;
        }

        var newScore = CalculateScore(
            artist.YoutubeViews90d,
            artist.InstagramReach90d,
            artist.TikTokPlays90d,
            artist.SpotifyListeners);

        artist.Score = newScore;
        artist.LastScoreUpdate = DateTime.UtcNow;
        await _artistRepository.UpdateAsync(artist, cancellationToken);

        _logger.LogInformation("Artist {Name} skoru güncellendi: {Score}", artist.Name, newScore);
    }

    public async Task RecalculateAllAsync(CancellationToken cancellationToken)
    {
        var artists = await _artistRepository.GetAsync(cancellationToken);
        _logger.LogInformation("Toplam {Count} artist skoru yeniden hesaplanıyor", artists.Count);

        foreach (var artist in artists.Where(a => !a.ManualOverride))
        {
            var newScore = CalculateScore(
                artist.YoutubeViews90d,
                artist.InstagramReach90d,
                artist.TikTokPlays90d,
                artist.SpotifyListeners);

            artist.Score = newScore;
            artist.LastScoreUpdate = DateTime.UtcNow;
            await _artistRepository.UpdateAsync(artist, cancellationToken);
        }

        _logger.LogInformation("Tüm artist skorları güncellendi");
    }

    private static double Normalize(long value, long max) =>
        max == 0 ? 0 : Math.Min(100.0, value / (double)max * 100.0);
}
