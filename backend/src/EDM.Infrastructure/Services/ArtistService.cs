using EDM.Application.Dtos.Artist;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;

namespace EDM.Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository _repo;
    public ArtistService(IArtistRepository repo) => _repo = repo;

    public async Task<List<ArtistResponseDto>> GetAsync(CancellationToken ct)
        => (await _repo.GetAsync(ct)).Select(MapToDto).ToList();

    public async Task<ArtistResponseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var artist = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Artist bulunamadi: {id}");
        return MapToDto(artist);
    }

    public async Task<ArtistResponseDto> CreateAsync(ArtistCreateDto dto, CancellationToken ct)
    {
        var artist = new Artist
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Score = dto.Score,
            LastScoreUpdate = DateTime.UtcNow,
            YoutubeViews90d = dto.YoutubeViews90d,
            InstagramReach90d = dto.InstagramReach90d,
            TikTokPlays90d = dto.TikTokPlays90d,
            SpotifyListeners = dto.SpotifyListeners
        };
        var created = await _repo.CreateAsync(artist, ct);
        return MapToDto(created);
    }

    public async Task<ArtistResponseDto> UpdateAsync(Guid id, ArtistUpdateDto dto, CancellationToken ct)
    {
        var artist = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Artist bulunamadi: {id}");
        if (dto.Name != null) artist.Name = dto.Name;
        if (dto.Score.HasValue) { artist.Score = dto.Score.Value; artist.LastScoreUpdate = DateTime.UtcNow; }
        if (dto.ManualOverride.HasValue) artist.ManualOverride = dto.ManualOverride.Value;
        if (dto.YoutubeViews90d.HasValue) artist.YoutubeViews90d = dto.YoutubeViews90d.Value;
        if (dto.InstagramReach90d.HasValue) artist.InstagramReach90d = dto.InstagramReach90d.Value;
        if (dto.TikTokPlays90d.HasValue) artist.TikTokPlays90d = dto.TikTokPlays90d.Value;
        if (dto.SpotifyListeners.HasValue) artist.SpotifyListeners = dto.SpotifyListeners.Value;
        var updated = await _repo.UpdateAsync(artist, ct);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var artist = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Artist bulunamadi: {id}");
        await _repo.DeleteAsync(artist, ct);
    }

    // dt=true → ManualOverride'ı kaldır (otomatik hesaplamaya geri dön)
    public async Task<ArtistResponseDto> OverrideScoreAsync(Guid id, int score, bool dt, CancellationToken ct)
    {
        var artist = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Artist bulunamadi: {id}");
        artist.Score = score;
        artist.ManualOverride = !dt;
        artist.LastScoreUpdate = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(artist, ct);
        return MapToDto(updated);
    }

    private static ArtistResponseDto MapToDto(Artist a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Slug = a.Slug,
        Score = a.Score,
        ManualOverride = a.ManualOverride,
        LastScoreUpdate = a.LastScoreUpdate,
        YoutubeViews90d = a.YoutubeViews90d,
        InstagramReach90d = a.InstagramReach90d,
        TikTokPlays90d = a.TikTokPlays90d,
        SpotifyListeners = a.SpotifyListeners,
        CreatedAt = a.CreationTime
    };
}
