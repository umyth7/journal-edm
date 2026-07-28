using System.ComponentModel.DataAnnotations;

namespace EDM.Application.Dtos.Artist;

public class ArtistCreateDto
{
    [Required][MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required][MaxLength(200)]
    public string Slug { get; set; } = string.Empty;
    [Range(1, 100)]
    public int Score { get; set; } = 50;
    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }
}
