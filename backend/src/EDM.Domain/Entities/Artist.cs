using EDM.Domain.Core;

namespace EDM.Domain.Entities;

public class Artist : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool ManualOverride { get; set; } = false;
    public DateTime LastScoreUpdate { get; set; }
    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }
    public ICollection<ContentJob> ContentJobs { get; set; } = new List<ContentJob>();
}
