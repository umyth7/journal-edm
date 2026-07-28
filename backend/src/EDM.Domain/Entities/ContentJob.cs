using EDM.Domain.Core;
using EDM.Domain.Enums;

namespace EDM.Domain.Entities;

public class ContentJob : BaseEntity<Guid>
{
    public Guid ArtistId { get; set; }
    public int Year { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string RawFileName { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public ContentJobStatus Status { get; set; } = ContentJobStatus.Pending;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? InstagramPostId { get; set; }
    public string? TikTokPostId { get; set; }
    public string? YoutubeVideoId { get; set; }
    public int RetryCount { get; set; } = 0;
    public Artist Artist { get; set; } = null!;
    public ICollection<Caption> Captions { get; set; } = new List<Caption>();
}
