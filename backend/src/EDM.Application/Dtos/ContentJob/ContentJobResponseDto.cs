using EDM.Domain.Enums;

namespace EDM.Application.Dtos.ContentJob;

public class ContentJobResponseDto
{
    public Guid Id { get; set; }
    public Guid ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public int Year { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string RawFileName { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public ContentJobStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? InstagramPostId { get; set; }
    public string? TikTokPostId { get; set; }
    public string? YoutubeVideoId { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
