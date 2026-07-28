using System.ComponentModel.DataAnnotations;

namespace EDM.Application.Dtos.Artist;

public class ArtistUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }
    [Range(1, 100)]
    public int? Score { get; set; }
    public bool? ManualOverride { get; set; }
    public long? YoutubeViews90d { get; set; }
    public long? InstagramReach90d { get; set; }
    public long? TikTokPlays90d { get; set; }
    public long? SpotifyListeners { get; set; }
}
