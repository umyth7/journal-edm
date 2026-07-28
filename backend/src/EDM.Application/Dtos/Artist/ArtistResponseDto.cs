namespace EDM.Application.Dtos.Artist;

public class ArtistResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool ManualOverride { get; set; }
    public DateTime LastScoreUpdate { get; set; }
    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }
    public DateTime CreatedAt { get; set; }
}
